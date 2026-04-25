using backend.Application.Abstractions.Users;
using backend.Data;
using backend.Models.Orders;
using backend.Infrastructure.Services;
using backend.Infrastructure.Services.Users;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), npgsqlOptions =>
            {
                npgsqlOptions.MapEnum<PaymentStatus>("payment_status");
                npgsqlOptions.MapEnum<OrderStatus>("order_status");
                npgsqlOptions.MapEnum<PaymentMethod>("payment_method");
                npgsqlOptions.MapEnum<PaymentTxnStatus>("payment_txn_status");
                npgsqlOptions.MapEnum<AddressType>("address_type");
            }));

        services.AddScoped<DataSeeder>();
        services.AddScoped<ITokenService, TokenService>();

        return services;
    }
}
