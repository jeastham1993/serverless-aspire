#pragma warning disable CA1812

using Microsoft.EntityFrameworkCore;
using ProductAPI.AzureFunctions.ProductManagement;

namespace ProductAPI.AzureFunctions.DataAccess;

internal sealed class PostgresProducts(ProductDbContext dbContext) : IProducts
{
    public async Task<Product?> WithId(string id)
    {
        var product = await dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == id);
        
        return product;
    }

    public async Task<Product> New(ProductName name, ProductPrice price)
    {
        var product = Product.Create(name, price);
        
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();
        
        return product;
    }

    public async Task Delete(string id)
    {
        var product = await WithId(id);
        if (product == null)
        {
            return;
        }

        dbContext.Products.Remove(product);
        await dbContext.SaveChangesAsync();
    }

    public async Task<ProductWrapper> All()
    {
        var products = await dbContext.Products
            .AsNoTracking()
            .ToListAsync();

        return new ProductWrapper(products);
    }
}