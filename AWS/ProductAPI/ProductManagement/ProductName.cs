namespace ProductAPI.ProductManagement;

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