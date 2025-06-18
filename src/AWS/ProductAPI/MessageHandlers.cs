// Unless explicitly stated otherwise all files in this repository are licensed under the Apache License Version 2.0.
// This product includes software developed at Datadog (https://www.datadoghq.com/).
// Copyright 2025 Datadog, Inc.

using Amazon.Lambda.Annotations;
using Amazon.Lambda.SQSEvents;

namespace ProductAPI;

public class MessageHandlers
{
    [LambdaFunction]
    public async Task HandleSqsMessage(SQSEvent evt)
    {
        foreach (var message in evt.Records)
        {
            // Process each message
            Console.WriteLine($"Received message: {message.Body}");
            
            // Here you can add logic to process the message, e.g., save to a database, etc.
            // For demonstration, we just log the message body.
        }
    }
}