using System.Text.Json.Serialization;

namespace ProductAPI.ProductManagement;

public record CreateProductRequest
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";

    [JsonPropertyName("price")] public decimal Price { get; set; } = 0;
}