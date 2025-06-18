// Unless explicitly stated otherwise all files in this repository are licensed under the Apache License Version 2.0.
// This product includes software developed at Datadog (https://www.datadoghq.com/).
// Copyright 2025 Datadog, Inc.

namespace ProductAPI.AzureFunctions.ProductManagement;

public record ProductName
{
    public string Value { get; }

    public ProductName(string value)
    {
        if (string.IsNullOrEmpty(value) || value.Length < 3 || value.Length > 50)
        {
            throw new ArgumentException("Product name must be between 3 and 50 characters long.");
        }
        
        this.Value = value;
    }
}