using backend.Data;
using backend.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<DataSeeder>();

        return services;
    }
}