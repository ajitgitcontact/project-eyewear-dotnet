using backend.Application.Abstractions.Products;
using backend.Application.Abstractions.Orders;
using backend.Application.Abstractions.Users;
using backend.Infrastructure.Services.Orders;
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
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IOrderItemService, OrderItemService>();
        services.AddScoped<IOrderItemCustomizationService, OrderItemCustomizationService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IOrderAddressService, OrderAddressService>();
        services.AddScoped<IOrderStatusLogService, OrderStatusLogService>();
        services.AddScoped<ICustomerPrescriptionService, CustomerPrescriptionService>();

        return services;
    }
}
