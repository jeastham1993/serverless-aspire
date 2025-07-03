#pragma warning disable CA1515, CA1812

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.SQSEvents;
using AWS.Lambda.Powertools.Logging;
using Datadog.Trace;
using Microsoft.Extensions.Logging;
using ProductAPI.ProductManagement;

namespace ProductAPI;

internal sealed record ProductPurchasedEvent(string ProductId, string OrderNumber);

[SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates")]
internal sealed class ProductPurchasedEventHandler(IProducts products)
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
            using var processTrace = Tracer.Instance.StartActive($"process product.reStocked.v1");

            try
            {
                var messageBody = JsonSerializer.Deserialize<ProductPurchasedEvent>(message.Body, _options);

                if (messageBody is null) throw new ArgumentException("Message body is null or invalid.");

                var product = await products.WithId(messageBody.ProductId);

                if (product is null) throw new ArgumentException($"Product with ID {messageBody.ProductId} not found.");

                product.ProductPurchased(messageBody.OrderNumber);

                await products.Update(product);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Logger.LogError(ex, "An error occured while processing message.");
                failures.Add(new SQSBatchResponse.BatchItemFailure
                {
                    ItemIdentifier = message.MessageId
                });
            }
            catch (ArgumentException ex)
            {
                Logger.LogError(ex, "An error occured while processing message.");
                failures.Add(new SQSBatchResponse.BatchItemFailure
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