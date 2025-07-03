// Unless explicitly stated otherwise all files in this repository are licensed under the Apache License Version 2.0.
// This product includes software developed at Datadog (https://www.datadoghq.com/).
// Copyright 2025 Datadog, Inc.

#pragma warning disable CA1812
#pragma warning disable CA1305

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.Lambda.SQSEvents;
using Amazon.Runtime;
using Aspire.Hosting.AWS.CDK;
using Aspire.Hosting.AWS.CloudFormation;
using Aspire.Hosting.AWS.DynamoDB;
using Aspire.Hosting.AWS.Lambda;
using Microsoft.Extensions.DependencyInjection;
using Serverless.Aspire.AWS.Localstack;

namespace Serverless.Aspire.AWS;

internal sealed record LambdaTestSqsMessage<T>(string QueueName, string FunctionName, T MessageBody);

internal sealed record LambdaCommonReferences(
    IResourceBuilder<IDynamoDBLocalResource> DynamoDbLocal,
    IResourceBuilder<LocalStackResource> LocalStack);

[SuppressMessage("Reliability", "CA2012:Use ValueTasks correctly")]
internal static class ResourceBuilderExtensions
{
    /// <summary>
    /// Add a test command button to the Aspire UI.
    /// </summary>
    /// <param name="builder">The Azure service bus resource.</param>
    /// <param name="command">The <see cref="ServiceBusTestCommand"/> to add to the UI.</param>
    /// <returns></returns>
    internal static IResourceBuilder<LambdaProjectResource> WithLambdaTestCommands<T>(
        this IResourceBuilder<LambdaProjectResource> builder, IResource lambdaServiceEmulatorResource,
        LambdaTestSqsMessage<T> command)
    {
        builder.ApplicationBuilder.Services.AddSingleton<AmazonLambdaClient>(provider =>
        {
            var endpoints = lambdaServiceEmulatorResource.TryGetEndpoints(out var endpointsList)
                ? endpointsList
                : throw new InvalidOperationException(
                    "Lambda service emulator does not have an HTTP endpoint configured.");

            var connectionString = endpoints.FirstOrDefault()!.AllocatedEndpoint!.UriString;
            return new AmazonLambdaClient(new BasicAWSCredentials("dummykey", "dummysecret"), new AmazonLambdaConfig
            {
                ServiceURL = connectionString
            });
        });

        builder.WithCommand(command.QueueName, $"Send to {command.QueueName}", async (c) =>
        {
            var lambdaClient = c.ServiceProvider.GetRequiredService<AmazonLambdaClient>();
            await lambdaClient.InvokeAsync(new InvokeRequest
            {
                FunctionName = command.FunctionName,
                Payload = JsonSerializer.Serialize(new SQSEvent
                {
                    Records = new List<SQSEvent.SQSMessage>
                    {
                        new()
                        {
                            Body = JsonSerializer.Serialize(command.MessageBody)
                        }
                    }
                })
            });

            return new ExecuteCommandResult { Success = true };
        }, new CommandOptions());

        return builder;
    }
    
    internal static IResourceBuilder<LambdaProjectResource> WithCommonReferences(
        this IResourceBuilder<LambdaProjectResource> builder, LambdaCommonReferences commonReferences)
    {
        builder
            .WaitFor(commonReferences.DynamoDbLocal)
            .WaitFor(commonReferences.LocalStack)
            .WithReference(commonReferences.DynamoDbLocal)
            .WithReference(commonReferences.LocalStack)
            .WithEnvironment("AWS_ENDPOINT_URL_SNS", () => commonReferences.LocalStack.GetEndpoint("http").Url)
            .WithEnvironment("PRODUCT_TABLE_NAME", "Products")
            .WithEnvironment("AWS_ACCESS_KEY_ID", "dummyaccesskey")
            .WithEnvironment("AWS_SECRET_ACCESS_KEY", "dummysecretaccesskey");

        return builder;
    }

    internal static Func<string> ExtractOutputValueFor(this IResourceBuilder<ICloudFormationTemplateResource> builder,
        string outputKey)
    {
        return () =>
        {
            var outputs = builder.Resource.Outputs;

            if (outputs is null) return "";

            var outputValue =
                outputs.FirstOrDefault(output => output.OutputKey == outputKey)?.OutputValue ??
                "";

            return outputValue;
        };
    }

    internal static Func<string> ExtractOutputValueFor(this IResourceBuilder<IStackResource> builder,
        string outputKey)
    {
        return () =>
        {
            var outputs = builder.Resource.Outputs;

            if (outputs is null) return "";

            var outputValue =
                outputs.FirstOrDefault(output => output.OutputKey == outputKey)?.OutputValue ??
                "";

            if (string.IsNullOrEmpty(outputValue))
                outputValue =
                    outputs.FirstOrDefault(output =>
                        output.OutputKey.Contains(outputKey, StringComparison.OrdinalIgnoreCase))?.OutputValue ?? "";

            return outputValue;
        };
    }
}