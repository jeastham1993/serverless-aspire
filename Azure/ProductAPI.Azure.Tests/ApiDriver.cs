using System.Net.Http.Json;

namespace ProductAPI.AzureFunctions.Tests;

public class ApiDriver
{
    private readonly HttpClient _httpClient;

    public ApiDriver(HttpClient httpClient)
    {
        _httpClient = httpClient;
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
}