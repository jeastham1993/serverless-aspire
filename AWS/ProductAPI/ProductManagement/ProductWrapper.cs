namespace ProductAPI.ProductManagement
{
    public class ProductWrapper
    {
        public ProductWrapper()
        {
            Products = new List<Product>();
        }

        public ProductWrapper(IReadOnlyCollection<Product> products)
        {
            Products = products;
        }
        
        public IReadOnlyCollection<Product> Products { get; }
    }
}