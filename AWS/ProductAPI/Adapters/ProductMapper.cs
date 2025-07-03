using System.Globalization;
using Amazon.DynamoDBv2.Model;
using ProductAPI.ProductManagement;

namespace ProductAPI.Adapters;

public static class ProductMapper
{
    public const string Pk = "id";
    public const string Name = "name";
    public const string Price = "price";
    public const string Stock = "stock";
    public const string PurchaseCount = "purchaseCount";

    public static Product ProductFromDynamoDb(Dictionary<string, AttributeValue> items)
    {
        ArgumentNullException.ThrowIfNull(items, nameof(items));

        var product = new Product(
            items[Pk].S,
            items[Name].S,
            decimal.Parse(items[Price].N, CultureInfo.InvariantCulture),
            decimal.Parse(items[Stock].N, CultureInfo.InvariantCulture),
            decimal.Parse(items[PurchaseCount].N, CultureInfo.InvariantCulture)
        );

        return product;
    }

    public static Dictionary<string, AttributeValue> ProductToDynamoDb(Product product)
    {
        ArgumentNullException.ThrowIfNull(product, nameof(product));
        var item = new Dictionary<string, AttributeValue>(3);
        item.Add(Pk, new AttributeValue(product.Id));
        item.Add(Name, new AttributeValue(product.Name));
        item.Add(Price, new AttributeValue
        {
            N = product.Price.ToString(CultureInfo.InvariantCulture)
        });
        item.Add(PurchaseCount, new AttributeValue
        {
            N = product.PurchaseCount.ToString(CultureInfo.InvariantCulture)
        });
        item.Add(Stock, new AttributeValue
        {
            N = product.StockLevel.ToString(CultureInfo.InvariantCulture)
        });

        return item;
    }
}