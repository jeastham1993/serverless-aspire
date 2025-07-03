// Unless explicitly stated otherwise all files in this repository are licensed under the Apache License Version 2.0.
// This product includes software developed at Datadog (https://www.datadoghq.com/).
// Copyright 2025 Datadog, Inc.

#pragma warning disable CS9113

namespace Serverless.Aspire.AWS.DatadogAspireExtensions;

internal sealed class DatadogResource(string name, DatadogOptions options)
    : ContainerResource(name), IResourceWithConnectionString
{
    internal const string PrimaryEndpointName = "apm";

    private EndpointReference? _primaryEndpoint;

    public EndpointReference PrimaryEndpoint => _primaryEndpoint ??= new EndpointReference(this, PrimaryEndpointName);

    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create($"{PrimaryEndpoint.Property(EndpointProperty.Url)}");

    public DatadogOptions Options => options;
}