// Unless explicitly stated otherwise all files in this repository are licensed under the Apache License Version 2.0.
// This product includes software developed at Datadog (https://www.datadoghq.com/).
// Copyright 2025 Datadog, Inc.

namespace Serverless.Aspire.AWS.DatadogAspireExtensions;

internal sealed record DatadogOptions(string DDApiKey)
{
    public string DDSite { get; set; } = "datadoghq.com";

    public string ServiceName { get; set; } = "";

    public string Environment { get; set; } = "local";

    public string Version { get; set; } = "local";

    public bool EnableLogs { get; set; }
    
    public bool EnableAPM { get; set; }
    
    public bool EnableDogStatsD { get; set; }

    public bool EnableRuntimeMetrics { get; set; }
    
    public bool EnableProfiling { get; set; }

    public bool EnaleOTELEndpoints { get; set; } = true;
}