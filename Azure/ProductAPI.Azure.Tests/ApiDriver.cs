using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;

#pragma warning disable CA1812

namespace ProductAPI.AzureFunctions.Tests;

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
    private readonly ServiceBusClient _serviceBusClient;

    public ApiDriver(HttpClient httpClient, ServiceBusClient serviceBusClient)
    {
        _httpClient = httpClient;
        _serviceBusClient = serviceBusClient;
    }

    public async Task<HttpResponseMessage> HealthCheckAsync()
    {
        var response = await _httpClient.GetAsync("/api/health");
        return response;
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

    public async Task InjectProductPurchasedMessageFor(string productId, string orderNumber)
    {
        var sender = _serviceBusClient.CreateSender("products.productPurchased.v1");
        var messageBody = JsonSerializer.Serialize(new
        {
            ProductId = productId,
            OrderNumber = orderNumber
        });
        var serviceBusMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(messageBody))
        {
            ContentType = "application/json"
        };

        await sender.SendMessageAsync(serviceBusMessage);
    }

    public async Task InjectProductRestockedMessageFor(string productId, decimal newStockLevel)
    {
        var sender = _serviceBusClient.CreateSender("products.productReStocked.v1");
        var messageBody = JsonSerializer.Serialize(new
        {
            ProductId = productId,
            NewStockLevel = newStockLevel
        });
        var serviceBusMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(messageBody))
        {
            ContentType = "application/json"
        };

        await sender.SendMessageAsync(serviceBusMessage);
    }
}