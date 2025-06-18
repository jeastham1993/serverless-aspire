namespace ProductAPI.ProductManagement
{
    public interface IProducts
    {
        Task<Product?> WithId(string id);

        Task<Product> AddNew(ProductName name, ProductPrice price);

        Task Delete(string id);

        Task<ProductWrapper> All();
    }
}