using System.Net.Http.Json;
using System.Text.Json;
using Xunit.Abstractions;

namespace ProductAPI.AzureFunctions.Tests;

public class ApiTests(TestSetupFixture setupFixture) : IClassFixture<TestSetupFixture>
{
    [Fact]
    public async Task CanCreateNewProductShouldReturn201()
    {
        var productName = Guid.NewGuid().ToString();
        var result = await setupFixture.ApiDriver.PostProductAsync(productName, 10);
        
        result.EnsureSuccessStatusCode();
        Assert.Equal(201, (int)result.StatusCode);
    }
    
    [Fact]
    public async Task CanCreateNewProductAndRetrieveShouldReturn200()
    {
        var productName = Guid.NewGuid().ToString();
        await setupFixture.ApiDriver.PostProductAsync(productName, 10);
        var getResult = await setupFixture.ApiDriver.GetProductAsync(productName.ToUpperInvariant().Replace(" ",  "", StringComparison.OrdinalIgnoreCase));
        
        Assert.Equal(200, (int)getResult.StatusCode);
    }
    
    [Fact]
    public async Task CanCreateNewProductAndRestock()
    {
        var productName = Guid.NewGuid().ToString();
        var result = await setupFixture.ApiDriver.PostProductAsync(productName, 10);
        var product = await result.Content.ReadFromJsonAsync<ProductDTO>() ??
                      throw new InvalidOperationException("Product not found in response.");
        
        result.EnsureSuccessStatusCode();

        await setupFixture.ApiDriver.InjectProductRestockedMessageFor(product.Id, 10);
        
        await Task.Delay(TimeSpan.FromSeconds(30));
        
        var getResult = await setupFixture.ApiDriver.GetProductAsync(product.Id);
        var responseBody = await getResult.Content.ReadAsStringAsync();
        product = JsonSerializer.Deserialize<ProductDTO>(responseBody, setupFixture.JsonSerializerOptions);
        
        Assert.Equal(10, product!.StockLevel);
    }
    
    [Fact]
    public async Task CanCreateNewProductAndPurchase()
    {
        var productName = Guid.NewGuid().ToString();
        var result = await setupFixture.ApiDriver.PostProductAsync(productName, 10);
        var product = await result.Content.ReadFromJsonAsync<ProductDTO>() ??
                      throw new InvalidOperationException("Product not found in response.");
        
        result.EnsureSuccessStatusCode();

        await setupFixture.ApiDriver.InjectProductPurchasedMessageFor(product.Id, "ORD1234");
        
        await Task.Delay(TimeSpan.FromSeconds(30));
        
        var getResult = await setupFixture.ApiDriver.GetProductAsync(product.Id);
        var responseBody = await getResult.Content.ReadAsStringAsync();
        product = JsonSerializer.Deserialize<ProductDTO>(responseBody, setupFixture.JsonSerializerOptions);
        
        Assert.Equal(1, product!.PurchaseCount);
    }
}