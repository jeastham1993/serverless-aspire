#pragma warning disable CA1515, CA1812

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using AWS.Lambda.Powertools.Logging;
using Microsoft.Extensions.Logging;
using ProductAPI.ProductManagement;

namespace ProductAPI;

internal sealed record ProductRestockedEvent(string ProductId, decimal NewStockLevel);

[SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates")]
internal sealed class ProductRestockedEventHandler(IProducts products)
{
    private JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    [LambdaFunction]
    public async Task<SQSBatchResponse> Handle(SQSEvent sqsEvent)
    {
        ArgumentNullException.ThrowIfNull(sqsEvent, nameof(sqsEvent));

        var failures = new List<SQSBatchResponse.BatchItemFailure>();
        
        foreach (var message in sqsEvent.Records)
        {
            try
            {
                var messageBody = JsonSerializer.Deserialize<ProductRestockedEvent>(message.Body, _options);

                if (messageBody is null) throw new ArgumentException("Message body is null or invalid.");

                var product = await products.WithId(messageBody.ProductId);

                if (product is null) throw new ArgumentException($"Product with ID {messageBody.ProductId} not found.");

                product.Restock(messageBody.NewStockLevel);

                await products.Update(product);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Logger.LogError(ex, "An error occured while processing message.");
                failures.Add(new  SQSBatchResponse.BatchItemFailure()
                {
                    ItemIdentifier = message.MessageId
                });
            }
            catch (ArgumentException ex)
            {
                Logger.LogError(ex, "An error occured while processing message.");
                failures.Add(new  SQSBatchResponse.BatchItemFailure()
                {
                    ItemIdentifier = message.MessageId
                });
            }
        }

        return new SQSBatchResponse
        {
            BatchItemFailures = failures
        };
    }
}