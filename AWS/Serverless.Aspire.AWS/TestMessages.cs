// Unless explicitly stated otherwise all files in this repository are licensed under the Apache License Version 2.0.
// This product includes software developed at Datadog (https://www.datadoghq.com/).
// Copyright 2025 Datadog, Inc.

#pragma warning disable CA1812

namespace Serverless.Aspire.AWS;

internal sealed record ProductRestockedTestMessage(string? ProductId, int NewStockLevel);

internal sealed record ProductPurchasedTestMessage(string ProductId, string OrderNumber);