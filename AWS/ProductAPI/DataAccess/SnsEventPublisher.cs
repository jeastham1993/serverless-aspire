// Unless explicitly stated otherwise all files in this repository are licensed under the Apache License Version 2.0.
// This product includes software developed at Datadog (https://www.datadoghq.com/).
// Copyright 2025 Datadog, Inc.

using Amazon.SimpleNotificationService;
using Microsoft.Extensions.Configuration;
using ProductAPI.ProductManagement;

namespace ProductAPI.DataAccess;

public class SnsEventPublisher(IConfiguration configuration, AmazonSimpleNotificationServiceClient snsClient) : IProductMessaging
{
    public async Task PublishProductCreated(string productId)
    {
        await snsClient.PublishAsync(configuration["PRODUCT_CREATED_TOPIC_ARN"], "hello");
    }
}