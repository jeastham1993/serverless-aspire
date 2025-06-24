namespace ProductAPI.AzureFunctions.ProductManagement
{
    internal sealed class Product
    {
        internal Product()
        {
            Id = string.Empty;
            Name = string.Empty;
        }

        internal Product(string id, string name, decimal price)
        {
            Id = id;
            Name = name;
            Price = price;
        }

        internal static Product Create(ProductName name, ProductPrice price)
        {
            return new Product(name.Value.ToUpperInvariant().Replace(" ", "", StringComparison.OrdinalIgnoreCase), name.Value, price.Value);
        }
        
        public string Id { get; set; }
        
        public string Name { get; set; }
        
        public decimal Price { get; private set; }
        
        public decimal PurchaseCount { get; private set; }
        
        public decimal StockLevel { get; private set; }

        public void SetPrice(decimal newPrice)
        {
            Price = Math.Round(newPrice, 2);
        }

        public void Restock(decimal newStockLevel)
        {
            if (newStockLevel < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(newStockLevel), "Stock level cannot be negative.");
            }
            StockLevel = Math.Round(newStockLevel, 2);
        }

        public void ProductPurchased(string orderNumber)
        {
            if (string.IsNullOrWhiteSpace(orderNumber))
            {
                throw new ArgumentException("Order number cannot be null or empty.", nameof(orderNumber));
            }
            // This is a naive implementation for demonstration purposes, in a real application you would
            // likely want to track orders in a more sophisticated way to make sure the same order isn't counted twice.
            PurchaseCount++;
        }
    }
}