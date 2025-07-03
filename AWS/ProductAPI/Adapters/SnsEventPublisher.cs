// Unless explicitly stated otherwise all files in this repository are licensed under the Apache License Version 2.0.
// This product includes software developed at Datadog (https://www.datadoghq.com/).
// Copyright 2025 Datadog, Inc.

using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Amazon.SimpleNotificationService;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.SystemTextJson;
using Microsoft.Extensions.Configuration;
using ProductAPI.ProductManagement;

namespace ProductAPI.Adapters;

public class SnsEventPublisher(
    IConfiguration configuration,
    AmazonSimpleNotificationServiceClient snsClient,
    JsonEventFormatter jsonEventFormatter)
    : IProductMessaging
{
    public async Task PublishProductCreated(string productId)
    {
        await PublishAsCloudEvent(configuration["PRODUCT_CREATED_TOPIC_ARN"]!, "product.created.v1", new { productId });
    }

    public async Task PublishProductDeleted(string productId)
    {
        await PublishAsCloudEvent(configuration["PRODUCT_DELETED_TOPIC_ARN"]!, "product.deleted.v1", new { productId });
    }

    private async Task PublishAsCloudEvent(string? topicArn, string eventType, object? data)
    {
        var cloudEvent = new CloudEvent
        {
            Source = new Uri("http://products.dev"),
            Type = eventType,
            Id = Guid.NewGuid().ToString(),
            Time = DateTimeOffset.Now,
            DataContentType = "application/json",
            Data = data
        };
        cloudEvent.Validate();

        var cloudEventJson =
            Encoding.UTF8.GetString(jsonEventFormatter.EncodeStructuredModeMessage(cloudEvent, out _).Span);

        await snsClient.PublishAsync(topicArn, cloudEventJson);
    }
}