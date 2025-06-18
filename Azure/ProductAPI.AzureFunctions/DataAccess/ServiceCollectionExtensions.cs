using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductAPI.AzureFunctions.ProductManagement;

namespace ProductAPI.AzureFunctions.DataAccess;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration)
    {
        // Register DbContext
        services.AddDbContext<ProductDbContext>(options =>
        {
            options.UseNpgsql(configuration["CONNECTION_STRING"] ?? throw new InvalidOperationException("'CONNECTION_STRING' is not configured."),
                npgsqlOptions => npgsqlOptions.MigrationsAssembly("ProductAPI.AzureFunctions"));
        });

        // Register repositories
        services.AddSingleton<IProducts, PostgresProducts>();

        return services;
    }
}