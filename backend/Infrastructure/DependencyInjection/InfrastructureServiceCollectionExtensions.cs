using backend.Application.Abstractions.Users;
using backend.Data;
using backend.Infrastructure.Services;
using backend.Infrastructure.Services.Users;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<DataSeeder>();
        services.AddScoped<ITokenService, TokenService>();

        return services;
    }
}