using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Azure.Messaging.ServiceBus;

namespace ProductAPI.AzureFunctions.Tests;

[SuppressMessage("Reliability", "CA2012:Use ValueTasks correctly")]
public class TestSetupFixture : IDisposable
{
    public ApiDriver ApiDriver { get; }
    public DistributedApplication? App { get; }

    public JsonSerializerOptions JsonSerializerOptions { get; } = new JsonSerializerOptions()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public TestSetupFixture()
    {
        var builder = DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Serverless_Aspire_Azure>()
            .GetAwaiter()
            .GetResult();
        
        App = builder.BuildAsync().GetAwaiter().GetResult();

        App.StartAsync().GetAwaiter().GetResult();
        
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(30));
        App.ResourceNotifications.WaitForResourceHealthyAsync(
                "functions",
                cts.Token)
            .GetAwaiter().GetResult();
        
        var httpClient = App.CreateHttpClient("functions", "http");
        ApiDriver = new ApiDriver(httpClient, new ServiceBusClient(App.GetConnectionStringAsync("messaging").GetAwaiter().GetResult()));
        
        // Ensure the API is ready before running tests
        var retryCount = 0;
        var maxRetries = 5;

        while (retryCount < maxRetries)
        {
            try
            {
                var result = ApiDriver.HealthCheckAsync().GetAwaiter().GetResult();
            
                if (result.IsSuccessStatusCode)
                {
                    break; // API is ready
                }
            }
            catch (HttpRequestException)
            {
                // Empty exception block to handle transient errors
            }
            
            retryCount++;
            Task.Delay(TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();
        } 
    }
    
    public void Dispose()
    {
        App?.StopAsync().GetAwaiter().GetResult();
        GC.SuppressFinalize(this);
    }
}