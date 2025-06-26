#pragma warning disable CA1822 // Non-static required by Lambda Annotations

using Amazon.DynamoDBv2;
using Amazon.Lambda.Annotations;
using Amazon.SimpleNotificationService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductAPI.DataAccess;
using ProductAPI.ProductManagement;

namespace ProductAPI;

[LambdaStartup]
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        services.AddSingleton(sp => new AmazonSimpleNotificationServiceClient());
        services.AddSingleton<IProductMessaging, SnsEventPublisher>();
        services.AddSingleton(sp => new AmazonDynamoDBClient());
        services.AddSingleton<IProducts, DynamoDbProducts>();
        services.AddSingleton<IConfiguration>(configuration);
    }
}