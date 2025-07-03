// Unless explicitly stated otherwise all files in this repository are licensed under the Apache License Version 2.0.
// This product includes software developed at Datadog (https://www.datadoghq.com/).
// Copyright 2025 Datadog, Inc.

namespace Serverless.Aspire.AWS.DatadogAspireExtensions;

internal sealed record DatadogOptions
{
    internal const string DefaultDatadogSite = "datadoghq.com";
    internal const string DefaultEnvironment = "local";
    internal const string DefaultVersion = "latest";
    internal const string DefaultServiceName = "";
    
    public DatadogOptions(string ddApiKey)
    {
        // Attempt to pull the Datadog API key from the environment variables or configuration.
        if (string.IsNullOrEmpty(ddApiKey)) ddApiKey = System.Environment.GetEnvironmentVariable("DD_API_KEY") ?? "";
        
        ArgumentNullException.ThrowIfNull(ddApiKey, nameof(ddApiKey));
        
        DDApiKey = ddApiKey;
        DDSite = System.Environment.GetEnvironmentVariable("DD_SITE") ?? DefaultDatadogSite;
        ServiceName = System.Environment.GetEnvironmentVariable("DD_SERVICE") ?? DefaultServiceName;
        Environment = System.Environment.GetEnvironmentVariable("DD_ENV") ?? DefaultEnvironment;
        Version = System.Environment.GetEnvironmentVariable("DD_VERSION") ?? DefaultVersion;
    }
    
    public string DDApiKey { get; set; } = DefaultDatadogSite;
    
    public string DDSite { get; set; } = DefaultDatadogSite;

    public string ServiceName { get; set; } = DefaultServiceName;

    public string Environment { get; set; } = DefaultEnvironment;

    public string Version { get; set; } = DefaultVersion;

    public bool EnableLogs { get; set; }
    
    public bool EnableAPM { get; set; }
    
    public bool EnableDogStatsD { get; set; }

    public bool EnableRuntimeMetrics { get; set; }
    
    public bool EnableProfiling { get; set; }

    public bool EnaleOTELEndpoints { get; set; } = true;
}