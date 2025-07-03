// Unless explicitly stated otherwise all files in this repository are licensed under the Apache License Version 2.0.
// This product includes software developed at Datadog (https://www.datadoghq.com/).
// Copyright 2025 Datadog, Inc.

#pragma warning disable CS9113

using LocalStack.Client.Contracts;

namespace Serverless.Aspire.AWS.Localstack;

internal sealed class LocalStackResource(string name, ILocalStackOptions options) : ContainerResource(name), IResourceWithConnectionString
{
    internal const string PrimaryEndpointName = "http";

    private EndpointReference? _primaryEndpoint;

    public EndpointReference PrimaryEndpoint => _primaryEndpoint ??= new(this, PrimaryEndpointName);

    public ReferenceExpression ConnectionStringExpression => ReferenceExpression.Create($"{PrimaryEndpoint.Property(EndpointProperty.Url)}");
}