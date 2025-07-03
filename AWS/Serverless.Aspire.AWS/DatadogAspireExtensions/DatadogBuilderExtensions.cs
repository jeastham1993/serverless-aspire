// Unless explicitly stated otherwise all files in this repository are licensed under the Apache License Version 2.0.
// This product includes software developed at Datadog (https://www.datadoghq.com/).
// Copyright 2025 Datadog, Inc.

namespace Serverless.Aspire.AWS.DatadogAspireExtensions;

internal static class DatadogBuilderExtensions
{
    public static IResourceBuilder<DatadogResource> AddDatadog(this IDistributedApplicationBuilder builder,
        DatadogOptions options)
    {
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        ArgumentNullException.ThrowIfNull(options.DDApiKey, nameof(options.DDApiKey));

        var datadogResource = new DatadogResource("datadog-agent", options);

        var container = builder.AddResource(datadogResource)
            .WithImage(DatadogContainerImageTags.Image, DatadogContainerImageTags.Tag)
            .WithImageRegistry(DatadogContainerImageTags.Registry)
            .WithEnvironment("DD_API_KEY", options.DDApiKey)
            .WithEnvironment("DD_SITE", options.DDSite)
            .WithEnvironment("DD_AGENT_HOST", "dd-agent")
            .WithEnvironment("DD_HOSTNAME_TRUST_UTS_NAMESPACE", "true")
            .WithEnvironment("DD_LOG_LEVEL", "ERROR")
            .WithLifetime(ContainerLifetime.Persistent);

        if (options.EnableAPM)
            container.WithEnvironment("DD_APM_ENABLED", "true")
                .WithEnvironment("DD_APM_NON_LOCAL_TRAFFIC", "true")
                .WithEndpoint(8126, 8126, name: "apm"); // APM port

        if (options.EnableLogs)
            container.WithEnvironment("DD_LOGS_ENABLED", "true")
                .WithEndpoint(10518, 10518, name: "logs");

        if (options.EnableDogStatsD)
            container.WithEnvironment("DD_DOGSTATSD_NON_LOCAL_TRAFFIC", "true")
                .WithEndpoint(8125, 8125, name: "dogstatsd");

        if (options.EnaleOTELEndpoints)
            container.WithEnvironment("DD_OTLP_CONFIG_RECEIVER_PROTOCOLS_GRPC_ENDPOINT", "0.0.0.0:4317")
                .WithEnvironment("DD_OTLP_CONFIG_RECEIVER_PROTOCOLS_HTTP_ENDPOINT", "0.0.0.0:4318")
                .WithEndpoint(4317, 4317, name: "otel-grpc")
                .WithEndpoint(4318, 4318, name: "otel-http");

        return container;
    }
}