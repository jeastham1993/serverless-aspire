// Unless explicitly stated otherwise all files in this repository are licensed under the Apache License Version 2.0.
// This product includes software developed at Datadog (https://www.datadoghq.com/).
// Copyright 2025 Datadog, Inc.

namespace ProductAPI.Models;

public record ProductPrice
{
    public decimal Value { get; }

    public ProductPrice(decimal value)
    {
        if (value <= 0)
        {
            throw new ArgumentException("Product price must be greater than zero.");
        }
        
        this.Value = value;
    }
}