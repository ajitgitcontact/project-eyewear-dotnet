using backend.Data;
using backend.Services.UserService;
using backend.Services.ProductsService.Interfaces;
using backend.Services.ProductsService.Services;
using Microsoft.EntityFrameworkCore;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICustomizationOptionService, CustomizationOptionService>();
builder.Services.AddScoped<ICustomizationValueService, CustomizationValueService>();
builder.Services.AddScoped<IProductImageService, ProductImageService>();
builder.Services.AddScoped<ICustomizationImageService, CustomizationImageService>();
builder.Services.AddScoped<IProductBusinessService, ProductBusinessService>();

var app = builder.Build();

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
app.UseStaticFiles();
app.MapControllers();

app.Run();
