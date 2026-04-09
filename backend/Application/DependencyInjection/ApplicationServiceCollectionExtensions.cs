using backend.Application.Abstractions.Products;
using backend.Application.Abstractions.Users;
using backend.Infrastructure.Services.Products;
using backend.Infrastructure.Services.Users;

namespace backend.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICustomizationOptionService, CustomizationOptionService>();
        services.AddScoped<ICustomizationValueService, CustomizationValueService>();
        services.AddScoped<IProductImageService, ProductImageService>();
        services.AddScoped<ICustomizationImageService, CustomizationImageService>();
        services.AddScoped<IProductBusinessService, ProductBusinessService>();

        return services;
    }
}