#pragma warning disable CA1812

using System.Text.Json.Serialization;

namespace ProductAPI.AzureFunctions.ProductManagement;

internal sealed record CreateProductRequest
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";

    [JsonPropertyName("price")] public decimal Price { get; set; } = 0;
}