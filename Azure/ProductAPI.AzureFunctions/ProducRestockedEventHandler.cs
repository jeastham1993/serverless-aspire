#pragma warning disable CA1515, CA1812

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using ProductAPI.AzureFunctions.ProductManagement;

namespace ProductAPI.AzureFunctions;

internal sealed record ProductRestockedEvent(string ProductId, decimal NewStockLevel);

[SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates")]
internal sealed class ProductRestockedEventHandler(IProducts products, ILogger<ProductRestockedEventHandler> logger)
{
    private JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };
    
    [Function(nameof(ProductRestockedEventHandler))]
    public async Task Run(
        [ServiceBusTrigger("products.productReStocked.v1", Connection = "SERVICE_BUS_CONNECTION_STRING")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        ArgumentNullException.ThrowIfNull(message);

        try
        {
            var messageBody = JsonSerializer.Deserialize<ProductRestockedEvent>(message.Body, _options);
            
            if (messageBody is null)
            {
                throw new ArgumentException("Message body is null or invalid.");
            }
            
            var product = await products.WithId(messageBody.ProductId);
            
            if (product is null)
            {
                throw new ArgumentException($"Product with ID {messageBody.ProductId} not found.");
            }
            
            product.Restock(messageBody.NewStockLevel);
            
            await products.Update(product);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: "The data provided in the message is out of range.", deadLetterErrorDescription: ex.Message);
            logger.LogError(ex, "An error occured while processing message.");
        }
        catch (ArgumentException ex)
        {
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: "The data provided in the message is invalid.", deadLetterErrorDescription: ex.Message);
            logger.LogError(ex, "An error occured while processing message.");
        }
    }
}