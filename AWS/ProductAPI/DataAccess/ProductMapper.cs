using System.Globalization;
using Amazon.DynamoDBv2.Model;
using ProductAPI.ProductManagement;

namespace ProductAPI.DataAccess
{
    public static class ProductMapper
    {
        public const string Pk = "id";
        public const string Name = "name";
        public const string Price = "price";
        
        public static Product ProductFromDynamoDb(Dictionary<String, AttributeValue> items) {
            ArgumentNullException.ThrowIfNull(items, nameof(items));
            
            var product = new Product(items[Pk].S, items[Name].S, decimal.Parse(items[Price].N, CultureInfo.InvariantCulture));

            return product;
        }
        
        public static Dictionary<String, AttributeValue> ProductToDynamoDb(Product product) {
            ArgumentNullException.ThrowIfNull(product, nameof(product));
            Dictionary<String, AttributeValue> item = new Dictionary<string, AttributeValue>(3);
            item.Add(Pk, new AttributeValue(product.Id));
            item.Add(Name, new AttributeValue(product.Name));
            item.Add(Price, new AttributeValue()
            {
                N = product.Price.ToString(CultureInfo.InvariantCulture)
            });

            return item;
        }
    }
}