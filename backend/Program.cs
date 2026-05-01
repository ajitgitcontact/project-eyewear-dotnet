using System.Text;
using System.Text.Json.Serialization;
using backend.Application.DependencyInjection;
using backend.Infrastructure.DependencyInjection;
using backend.Infrastructure.Services;
using backend.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfiguration) =>
    loggerConfiguration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes["BearerAuth"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "JWT Bearer authentication. Enter only the token value."
        };

        return Task.CompletedTask;
    });

    options.AddOperationTransformer((operation, context, cancellationToken) =>
    {
        var hasAuthorize = context.Description.ActionDescriptor.EndpointMetadata.OfType<AuthorizeAttribute>().Any();
        var hasAllowAnonymous = context.Description.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
        var routeKey = $"{context.Description.HttpMethod?.ToUpperInvariant()} /{context.Description.RelativePath}";
        var summaries = new Dictionary<string, string>
        {
            ["POST /api/Auth/signup"] = "Customer signup",
            ["POST /api/users/login"] = "User login and JWT issue",
            ["GET /api/products"] = "List products with nested customization and image data",
            ["POST /api/products"] = "Create a full product with customizations and images",
            ["POST /api/orders/create"] = "Create a complete checkout order",
            ["GET /api/orders"] = "Admin order search",
            ["GET /api/orders/{customerOrderId}"] = "Fetch complete order by CustomerOrderId",
            ["GET /api/orders/{customerOrderId}/logs"] = "Fetch admin order journey logs",
            ["GET /api/customer/orders"] = "List authenticated customer's orders",
            ["GET /api/admin/discounts"] = "List admin discounts",
            ["GET /api/admin/discounts/{id}"] = "Get admin discount by id",
            ["POST /api/admin/discounts"] = "Create admin discount",
            ["PUT /api/admin/discounts/{id}"] = "Update admin discount",
            ["DELETE /api/admin/discounts/{id}"] = "Delete admin discount",
            ["GET /api/admin/coupons"] = "List coupons",
            ["GET /api/admin/coupons/{id}"] = "Get coupon by id",
            ["POST /api/admin/coupons"] = "Create coupon",
            ["PUT /api/admin/coupons/{id}"] = "Update coupon",
            ["DELETE /api/admin/coupons/{id}"] = "Delete coupon",
            ["GET /api/cart"] = "Get active customer cart",
            ["POST /api/cart/items"] = "Add item to active cart",
            ["PUT /api/cart/items/{cartItemId}"] = "Update cart item quantity",
            ["DELETE /api/cart/items/{cartItemId}"] = "Remove cart item",
            ["DELETE /api/cart/clear"] = "Clear active cart",
            ["POST /api/cart/apply-coupon"] = "Apply coupon preview to cart",
            ["DELETE /api/cart/remove-coupon"] = "Remove cart coupon preview",
            ["POST /api/cart/checkout"] = "Checkout active cart into an order",
            ["GET /api/wishlist"] = "Get customer wishlist",
            ["POST /api/wishlist/items"] = "Add product to wishlist",
            ["DELETE /api/wishlist/items/{wishlistItemId}"] = "Remove wishlist item",
            ["POST /api/wishlist/items/{wishlistItemId}/move-to-cart"] = "Move wishlist item to cart"
        };

        if (summaries.TryGetValue(routeKey, out var summary))
            operation.Summary = summary;

        var authorizeRoles = context.Description.ActionDescriptor.EndpointMetadata
            .OfType<AuthorizeAttribute>()
            .Select(a => a.Roles)
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .ToList();

        if (hasAuthorize && !hasAllowAnonymous)
        {
            var roleDescription = authorizeRoles.Count > 0
                ? $"Required roles: {string.Join(" or ", authorizeRoles)}."
                : "Requires authenticated Bearer JWT.";
            operation.Description = string.IsNullOrWhiteSpace(operation.Description)
                ? roleDescription
                : $"{operation.Description} {roleDescription}";
        }

        if (hasAuthorize && !hasAllowAnonymous)
        {
            operation.Security ??= new List<OpenApiSecurityRequirement>();
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "BearerAuth"
                    }
                }] = Array.Empty<string>()
            });
        }

        return Task.CompletedTask;
    });
});

// ─── CORS Configuration ────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001", "http://192.168.1.8:3000", "http://192.168.1.8:3001")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services
    .AddInfrastructureLayer(builder.Configuration)
    .AddApplicationLayer();

// ─── JWT Authentication ────────────────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT key is not configured. Set JWT__Key in .env.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };

        // Return consistent JSON for 401 (unauthenticated) and 403 (forbidden)
        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(
                    System.Text.Json.JsonSerializer.Serialize(new { message = "Authentication required. Please provide a valid Bearer token." }));
            },
            OnForbidden = async context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(
                    System.Text.Json.JsonSerializer.Serialize(new { message = "You do not have permission to access this resource." }));
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();
app.UseMiddleware<GlobalExceptionMiddleware>();

// Enable CORS
app.UseCors("AllowFrontend");

// Seed initial data in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    await seeder.SeedSuperAdminAsync();
    await seeder.SeedProductsAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Eyewear API v1");
        options.DocumentTitle = "Eyewear API - Swagger";
    });
}

app.UseHttpsRedirection();
// app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
