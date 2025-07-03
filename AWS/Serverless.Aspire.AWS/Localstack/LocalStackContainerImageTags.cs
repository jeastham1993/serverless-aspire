// Unless explicitly stated otherwise all files in this repository are licensed under the Apache License Version 2.0.
// This product includes software developed at Datadog (https://www.datadoghq.com/).
// Copyright 2025 Datadog, Inc.

namespace Serverless.Aspire.AWS.Localstack;

internal static class LocalStackContainerImageTags
{
    public const string Registry = "docker.io";
    public const string Image = "localstack/localstack";
    public const string Tag = "3.4.0";
}