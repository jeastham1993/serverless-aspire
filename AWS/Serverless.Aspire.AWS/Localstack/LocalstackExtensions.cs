// Unless explicitly stated otherwise all files in this repository are licensed under the Apache License Version 2.0.
// This product includes software developed at Datadog (https://www.datadoghq.com/).
// Copyright 2025 Datadog, Inc.

using Amazon.CloudFormation;
using Aspire.Hosting.AWS.CDK;
using Aspire.Hosting.AWS.CloudFormation;
using LocalStack.Client;
using LocalStack.Client.Contracts;
using LocalStack.Client.Options;

namespace Serverless.Aspire.AWS.Localstack;

internal static class LocalStackExtensions
{
    public static IResourceBuilder<T> WithLocalStack<T>(this IResourceBuilder<T> builder, ILocalStackOptions? options = null) where T : ICloudFormationTemplateResource
    {
        ILocalStackOptions localStackOptions = options ?? new LocalStackOptions();

        if (!localStackOptions.UseLocalStack)
        {
            return builder;
        }

        var amazonCloudFormationClient = SessionStandalone.Init()
            .WithSessionOptions(localStackOptions.Session)
            .WithConfigurationOptions(localStackOptions.Config)
            .Create()
            .CreateClientByImplementation<AmazonCloudFormationClient>();

        builder.Resource.CloudFormationClient = amazonCloudFormationClient;

        return builder;
    }
}