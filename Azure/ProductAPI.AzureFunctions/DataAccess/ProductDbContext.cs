#pragma warning disable CA1812

using Microsoft.EntityFrameworkCore;
using ProductAPI.AzureFunctions.ProductManagement;

namespace ProductAPI.AzureFunctions.DataAccess;

internal sealed class ProductDbContext : DbContext
{
    public DbSet<Product> Products { get; set; } = null!;

    public ProductDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>()
            .ToTable("Products")
            .HasKey(p => p.Id);
    }
}