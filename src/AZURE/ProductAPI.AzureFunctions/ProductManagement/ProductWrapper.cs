namespace ProductAPI.AzureFunctions.ProductManagement
{
    public class ProductWrapper
    {
        public ProductWrapper()
        {
            Products = new List<Product>();
        }

        public ProductWrapper(List<Product> products)
        {
            Products = products;
        }
        
        public List<Product> Products { get; set; }
    }
}