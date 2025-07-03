// Unless explicitly stated otherwise all files in this repository are licensed under the Apache License Version 2.0.
// This product includes software developed at Datadog (https://www.datadoghq.com/).
// Copyright 2025 Datadog, Inc.

using Microsoft.Extensions.Configuration;

namespace Serverless.Aspire.AWS.DatadogAspireExtensions;

internal sealed class DatadogOptionsBuilder
{
    private DatadogOptions options;

    public DatadogOptionsBuilder(string ddApiKey)
    {
        options = new DatadogOptions(ddApiKey);
    }

    public DatadogOptionsBuilder WithDDSite(string ddSite)
    {
        options.DDSite = ddSite;
        return this;
    }

    public DatadogOptionsBuilder WithServiceName(string serviceName)
    {
        options.ServiceName = serviceName;
        return this;
    }

    public DatadogOptionsBuilder WithEnvironment(string environment)
    {
        options.Environment = environment;
        return this;
    }

    public DatadogOptionsBuilder WithVersion(string version)
    {
        options.Version = version;
        return this;
    }

    public DatadogOptionsBuilder WithLogs()
    {
        options.EnableLogs = true;
        return this;
    }

    public DatadogOptionsBuilder WithAPM()
    {
        options.EnableAPM = true;
        return this;
    }

    public DatadogOptionsBuilder WithDogStatsD()
    {
        options.EnableDogStatsD = true;
        return this;
    }

    public DatadogOptionsBuilder WithRuntimeMetrics()
    {
        options.EnableRuntimeMetrics = true;
        return this;
    }

    public DatadogOptionsBuilder WithProfiling()
    {
        options.EnableProfiling = true;
        return this;
    }

    public DatadogOptionsBuilder WithOTELEndpoints()
    {
        options.EnaleOTELEndpoints = true;
        return this;
    }

    public DatadogOptions Build()
    {
        return options;
    }
}