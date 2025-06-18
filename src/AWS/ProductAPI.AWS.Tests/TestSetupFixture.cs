// Unless explicitly stated otherwise all files in this repository are licensed under the Apache License Version 2.0.
// This product includes software developed at Datadog (https://www.datadoghq.com/).
// Copyright 2025 Datadog, Inc.

using Amazon.Lambda;
using Aspire.Hosting;
using Aspire.Hosting.Testing;

namespace ProductAPI.AWS.Tests;

public class TestSetupFixture : IDisposable
{
    public readonly ApiDriver ApiDriver;
    public readonly DistributedApplication? App;

    public TestSetupFixture()
    {
        var builder = DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Serverless_Aspire_AWS>()
            .GetAwaiter()
            .GetResult();
        
        App = builder.BuildAsync().GetAwaiter().GetResult();

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
    }
}