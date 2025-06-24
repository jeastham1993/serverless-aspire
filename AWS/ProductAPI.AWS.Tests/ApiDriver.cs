using System.Net.Http.Json;
using System.Text.Json;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.Servicecatalog;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.Lambda.SQSEvents;
using Amazon.Runtime;

#pragma warning disable CA1812

namespace ProductAPI.AWS.Tests;

internal sealed record ProductDTO
{
    public string Id { get; set; } = "";
        
    public string Name { get; set; } = "";
        
    public decimal Price { get; set; }
        
    public decimal PurchaseCount { get; set; }
        
    public decimal StockLevel { get; set; }
}

public class ApiDriver
{
    private readonly HttpClient _httpClient;
    private readonly AmazonLambdaClient _lambdaClient;
    private readonly string HandleProductPurchasedMessageFunctionName = "ProductPurchasedEventHandler";
    private readonly string HandleProductRestockedMessageFunctionName = "ProductRestockedEventHandler";

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
        var result = await _lambdaClient.InvokeAsync(new InvokeRequest
        {
            FunctionName = "HandleSQSMessageFunction",
            Payload = JsonSerializer.Serialize(sqsEvent)
        });

        return string.IsNullOrEmpty(result.FunctionError);
    }

    public async Task InjectProductPurchasedMessageFor(string productId, string orderNumber)
    {
        var messageBody = JsonSerializer.Serialize(new
        {
            ProductId = productId,
            OrderNumber = orderNumber
        });
        var sqsEvent = new SQSEvent
        {
            Records = new List<SQSEvent.SQSMessage>
            {
                new()
                {
                    Body = messageBody
                }
            }
        };
        var result = await _lambdaClient.InvokeAsync(new InvokeRequest
        {
            FunctionName = HandleProductPurchasedMessageFunctionName,
            Payload = JsonSerializer.Serialize(sqsEvent)
        });
    }

    public async Task InjectProductRestockedMessageFor(string productId, decimal newStockLevel)
    {
        var messageBody = JsonSerializer.Serialize(new
        {
            ProductId = productId,
            NewStockLevel = newStockLevel
        });
        var sqsEvent = new SQSEvent
        {
            Records = new List<SQSEvent.SQSMessage>
            {
                new()
                {
                    Body = messageBody
                }
            }
        };
        var result = await _lambdaClient.InvokeAsync(new InvokeRequest
        {
            FunctionName = HandleProductRestockedMessageFunctionName,
            Payload = JsonSerializer.Serialize(sqsEvent)
        });
    }
}