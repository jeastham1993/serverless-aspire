// Unless explicitly stated otherwise all files in this repository are licensed under the Apache License Version 2.0.
// This product includes software developed at Datadog (https://www.datadoghq.com/).
// Copyright 2025 Datadog, Inc.

using System.Runtime.InteropServices;
using Amazon.CloudFormation;
using Aspire.Hosting.AWS.CloudFormation;
using LocalStack.Client;
using LocalStack.Client.Contracts;
using LocalStack.Client.Options;

namespace Serverless.Aspire.AWS.DatadogAspireExtensions;

internal static class DatadogExtensions
{
    public static IResourceBuilder<T> WithDatadog<T>(this IResourceBuilder<T> builder, DatadogOptions options)
        where T : IResourceWithEnvironment
    {
        var osFolder = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "win"
            : RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                ? "osx"
                : "linux";
        var extension = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "dll"
            : RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                ? "dylib"
                : "so";
        var archName = "x64";

        if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
            archName = "x64";
        else if (RuntimeInformation.ProcessArchitecture == Architecture.X86)
            archName = "x86";
        else if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ||
                 RuntimeInformation.ProcessArchitecture == Architecture.Arm)
            archName = "arm64";

        var datadogTraceFolder = osFolder == "osx" ? osFolder : $"{osFolder}-{archName}";
        
        var tracerDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "bin", "Debug", "net8.0", "datadog");

        if (Directory.GetCurrentDirectory().Contains("bin", StringComparison.OrdinalIgnoreCase))
        {
            tracerDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "datadog");
        }
        
        var profilerFilePath =
            Path.Combine(tracerDirectoryPath, datadogTraceFolder, $"Datadog.Trace.ClrProfiler.Native.{extension}");

        if (!Directory.Exists(tracerDirectoryPath))
            throw new ArgumentException(
                $"Could not find Datadog tracer directory at {tracerDirectoryPath}, please ensure you have added the `Datadog.Trace.Bundle` package to your Aspire AppHost project and");

        if (!File.Exists(profilerFilePath))
            throw new ArgumentException(
                $"Could not find the Datadog profiler at {profilerFilePath}, please ensure you have added the `Datadog.Trace.Bundle` package to your Aspire AppHost project and");

        builder.WithEnvironment("CORECLR_ENABLE_PROFILING", "1")
            .WithEnvironment("CORECLR_PROFILER", "{846F5F1C-F9AE-4B07-969E-05C26BC060D8}")
            .WithEnvironment("CORECLR_PROFILER_PATH", profilerFilePath)
            .WithEnvironment("DD_DOTNET_TRACER_HOME", tracerDirectoryPath);

        if (options.EnableLogs) builder.WithEnvironment("DD_LOGS_INJECTION", "true");
        if (options.EnableRuntimeMetrics) builder.WithEnvironment("DD_RUNTIME_METRICS_ENABLED", "true");
        if (options.EnableProfiling)
        {
            builder.WithEnvironment("DD_PROFILING_ENABLED", "1");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var profilingFilePath =
                    Path.Combine(tracerDirectoryPath, datadogTraceFolder,
                        $"Datadog.Linux.ApiWrapper.{archName}.{extension}");

                if (!File.Exists(profilingFilePath))
                    throw new ArgumentException(
                        $"Could not find the Linux profiling API at {profilingFilePath}, please ensure you have added the `Datadog.Trace.Bundle` package to your Aspire AppHost project and");

                builder.WithEnvironment("LD_PRELOAD", profilingFilePath);
            }
        }

        return builder;
    }
}