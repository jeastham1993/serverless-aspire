using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Configuration;
using ProductAPI.ProductManagement;

namespace ProductAPI.Adapters
{
    public class DynamoDbProducts(AmazonDynamoDBClient dynamoDbClient, IConfiguration configuration) : IProducts
    {
        public async Task<Product?> WithId(string id)
        {
            var getItemResponse = await dynamoDbClient.GetItemAsync(new GetItemRequest(configuration["PRODUCT_TABLE_NAME"],
                new Dictionary<string, AttributeValue>(1)
                {
                    {ProductMapper.Pk, new AttributeValue(id)}
                }));

            return getItemResponse.IsItemSet ? ProductMapper.ProductFromDynamoDb(getItemResponse.Item) : null;
        }

        public async Task<Product> AddNew(ProductName name, ProductPrice price)
        {
            ArgumentNullException.ThrowIfNull(name, nameof(name));
            ArgumentNullException.ThrowIfNull(price, nameof(price));
            
            var product = Product.Create(name, price);
            
            await dynamoDbClient.PutItemAsync(configuration["PRODUCT_TABLE_NAME"], ProductMapper.ProductToDynamoDb(product));

            return product;
        }

        public async Task Delete(string id)
        {
            await dynamoDbClient.DeleteItemAsync(configuration["PRODUCT_TABLE_NAME"], new Dictionary<string, AttributeValue>(1)
            {
                {ProductMapper.Pk, new AttributeValue(id)}
            });
        }

        public async Task Update(Product product)
        {
            ArgumentNullException.ThrowIfNull(product, nameof(product));
            
            await dynamoDbClient.PutItemAsync(configuration["PRODUCT_TABLE_NAME"], ProductMapper.ProductToDynamoDb(product));
        }

        public async Task<ProductWrapper> All()
        {
            var data = await dynamoDbClient.ScanAsync(new ScanRequest()
            {
                TableName = configuration["PRODUCT_TABLE_NAME"],
                Limit = 20
            });

            var products = new List<Product>();

            if (data.Items != null)
            {
                foreach (var item in data.Items)
                {
                    products.Add(ProductMapper.ProductFromDynamoDb(item));
                }
            }

            return new ProductWrapper(products);
        }
    }
}