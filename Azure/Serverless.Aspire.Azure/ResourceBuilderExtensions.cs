// Unless explicitly stated otherwise all files in this repository are licensed under the Apache License Version 2.0.
// This product includes software developed at Datadog (https://www.datadoghq.com/).
// Copyright 2025 Datadog, Inc.

using System.Diagnostics.CodeAnalysis;
using Aspire.Hosting.Azure;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;

namespace Serverless.Aspire.Azure;

internal sealed record ServiceBusTestCommand(string QueueName, string MessageBody);

[SuppressMessage("Reliability", "CA2012:Use ValueTasks correctly")]
internal static class ResourceBuilderExtensions
{
    /// <summary>
    /// Add a test command button to the Aspire UI.
    /// </summary>
    /// <param name="builder">The Azure service bus resource.</param>
    /// <param name="command">The <see cref="ServiceBusTestCommand"/> to add to the UI.</param>
    /// <returns></returns>
    internal static IResourceBuilder<AzureServiceBusQueueResource> WithServiceBusTestCommand(
        this IResourceBuilder<AzureServiceBusQueueResource> builder, ServiceBusTestCommand command)
    {
        builder.ApplicationBuilder.Services.AddSingleton<ServiceBusClient>(provider =>
        {
            var connectionString = builder.Resource.Parent.ConnectionStringExpression
                .GetValueAsync(CancellationToken.None).GetAwaiter().GetResult();
            return new ServiceBusClient(connectionString);
        });

        builder.WithCommand(command.QueueName, $"Send to {command.QueueName}", async (c) =>
        {
            var sbClient = c.ServiceProvider.GetRequiredService<ServiceBusClient>();
            await sbClient.CreateSender(builder.Resource.QueueName)
                .SendMessageAsync(new ServiceBusMessage(command.MessageBody));

            return new ExecuteCommandResult { Success = true };
        }, new CommandOptions());

        return builder;
    }
}