namespace ProductAPI.ProductManagement;

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