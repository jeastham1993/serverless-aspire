// Unless explicitly stated otherwise all files in this repository are licensed under the Apache License Version 2.0.
// This product includes software developed at Datadog (https://www.datadoghq.com/).
// Copyright 2025 Datadog, Inc.

using System.Net.Http.Json;
using System.Text.Json;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.Servicecatalog;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.Lambda.SQSEvents;
using Amazon.Runtime;

namespace ProductAPI.AWS.Tests;

public class ApiDriver
{
    private readonly HttpClient _httpClient;
    private readonly AmazonLambdaClient _lambdaClient;

    public ApiDriver(HttpClient httpClient, AmazonLambdaClient lambdaClient)
    {
        _httpClient = httpClient;
        _lambdaClient = lambdaClient;
    }
    
    public async Task<HttpResponseMessage> GetProductsAsync()
    {
        var response = await _httpClient.GetAsync("/api/products");
        return response;
    }

    public async Task<HttpResponseMessage> GetProductAsync(string id)
    {
        var response = await _httpClient.GetAsync($"/api/products/{id}");
        return response;
    }

    public async Task<HttpResponseMessage> PostProductAsync(string name, decimal price)
    {
        var requestBody = new
        {
            Name = name,
            Price = price
        };
        
        var response = await _httpClient.PostAsJsonAsync("/api/products", requestBody);
        return response;
    }
    
    public async Task<HttpResponseMessage> DeleteProductAsync(string id)
    {
        var response = await _httpClient.DeleteAsync($"/api/products/{id}");
        return response;
    }

    public async Task<bool> HandleSqsMessage(string sqsMessageBody)
    {
        var sqsEvent = new SQSEvent
        {
            Records = new List<SQSEvent.SQSMessage>
            {
                new()
                {
                    Body = sqsMessageBody
                }
            }
        };
        var result = await _lambdaClient.InvokeAsync(new InvokeRequest()
        {
            FunctionName = "HandleSQSMessageFunction",
            Payload = JsonSerializer.Serialize(sqsEvent)
        });

        return string.IsNullOrEmpty(result.FunctionError);
    }
}