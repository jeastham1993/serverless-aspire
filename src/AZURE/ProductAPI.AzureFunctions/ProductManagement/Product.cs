namespace ProductAPI.AzureFunctions.ProductManagement
{
    public class Product
    {
        public Product()
        {
            Id = string.Empty;
            Name = string.Empty;
        }

        public Product(string id, string name, decimal price)
        {
            Id = id;
            Name = name;
            Price = price;
        }

        internal static Product Create(ProductName name, ProductPrice price)
        {
            return new Product(name.Value.ToUpper().Replace(" ", ""), name.Value, price.Value);
        }
        
        public string Id { get; set; }
        
        public string Name { get; set; }
        
        public decimal Price { get; private set; }

        public void SetPrice(decimal newPrice)
        {
            Price = Math.Round(newPrice, 2);
        }

        public override string ToString()
        {
            return "Product{" +
                   "id='" + Id + '\'' +
                   ", name='" + Name + '\'' +
                   ", price=" + Price +
                   '}';
        }
    }
}