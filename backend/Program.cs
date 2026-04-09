using backend.Application.DependencyInjection;
using backend.Infrastructure.DependencyInjection;
using backend.Middleware;
using Serilog;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfiguration) =>
    loggerConfiguration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services
    .AddInfrastructureLayer(builder.Configuration)
    .AddApplicationLayer();

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();
app.UseMiddleware<GlobalExceptionMiddleware>();

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
