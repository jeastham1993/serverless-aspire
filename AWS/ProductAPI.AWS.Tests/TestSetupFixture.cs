using System.Text.Json;
using Amazon.Lambda;
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.Configuration;

namespace ProductAPI.AWS.Tests;

public class TestSetupFixture : IDisposable
{
    public ApiDriver ApiDriver { get; init; }
    public DistributedApplication? App { get; init; }

    public JsonSerializerOptions JsonSerializerOptions { get; } = new JsonSerializerOptions()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public TestSetupFixture()
    {
        var builder = DistributedApplicationTestingBuilder
            // The Aspire test builder randomizes ports by default, which is useful for parallel test execution.
            // Disabling this ensures LocalStack and other services use the same port every time
            // This works for this scenario because we are starting Aspire as part of the test setup
            .CreateAsync<Projects.Serverless_Aspire_AWS>(["DcpPublisher:RandomizePorts=false"])
            .GetAwaiter()
            .GetResult();
        builder.Configuration
            .AddJsonFile("appsettings.development.json", optional: false, reloadOnChange: false)
            .AddEnvironmentVariables();
        
        App = builder
            .BuildAsync().GetAwaiter().GetResult();

        App.StartAsync().GetAwaiter().GetResult();
        
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(30));
        App.ResourceNotifications.WaitForResourceHealthyAsync(
                "APIGatewayEmulator",
                cts.Token)
            .GetAwaiter().GetResult();
        
        var httpClient = App.CreateHttpClient("APIGatewayEmulator", "http");
        
        var lambdaTestToolUrl = App.GetEndpoint("LambdaServiceEmulator", "http");
        ApiDriver = new ApiDriver(httpClient, new AmazonLambdaClient(new AmazonLambdaConfig()
        {
            ServiceURL = lambdaTestToolUrl.ToString(),
            DefaultAWSCredentials = new Amazon.Runtime.BasicAWSCredentials("dumykey", "dummysecret")
        }));
    }
    
    public void Dispose()
    {
        App?.StopAsync().GetAwaiter().GetResult();
        GC.SuppressFinalize(this);
    }
}