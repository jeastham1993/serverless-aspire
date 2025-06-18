// Unless explicitly stated otherwise all files in this repository are licensed under the Apache License Version 2.0.
// This product includes software developed at Datadog (https://www.datadoghq.com/).
// Copyright 2025 Datadog, Inc.

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductAPI.AzureFunctions.ProductManagement;

namespace ProductAPI.AzureFunctions.DataAccess;

public static class ServiceCollectionExtensions
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