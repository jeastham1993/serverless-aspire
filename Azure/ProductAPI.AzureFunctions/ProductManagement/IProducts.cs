namespace ProductAPI.AzureFunctions.ProductManagement
{
    internal interface IProducts
    {
        Task<Product?> WithId(string id);

        Task<Product> New(ProductName name, ProductPrice price);

        Task Delete(string id);

        Task<ProductWrapper> All();
    }
}