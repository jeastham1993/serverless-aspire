using Xunit.Abstractions;

namespace ProductAPI.AWS.Tests;

public class ApiTests(TestSetupFixture setupFixture) : IClassFixture<TestSetupFixture>
{
    [Fact]
    public async Task CanHandleSqsMessageShouldReturnOk()
    {
        var messageProcessingResult = await setupFixture.ApiDriver.HandleSqsMessage("Test SQS Message");
        
        Assert.True(messageProcessingResult, "SQS message processing should return true");
    }
    
    [Fact]
    public async Task CanCreateNewProductShouldReturn201()
    {
        var productName = "Test Product";
        var result = await setupFixture.ApiDriver.PostProductAsync(productName, 10);
        
        result.EnsureSuccessStatusCode();
        Assert.Equal(201, (int)result.StatusCode);
    }
    
    [Fact]
    public async Task CanCreateNewProductAndRetrieveShouldReturn200()
    {
        var productName = "Test Product";
        await setupFixture.ApiDriver.PostProductAsync(productName, 10);
        var getResult = await setupFixture.ApiDriver.GetProductAsync(productName.ToUpperInvariant().Replace(" ",  "", StringComparison.OrdinalIgnoreCase));
        
        Assert.Equal(200, (int)getResult.StatusCode);
    }
}