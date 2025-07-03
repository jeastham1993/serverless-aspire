using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Aspire.Hosting.AWS.DynamoDB;
using Aspire.Hosting.AWS.Lambda;
using Projects;
using Serverless.Aspire.AWS;

#pragma warning disable CA2252 // Opt in to preview features

var builder = DistributedApplication.CreateBuilder(args);

var dynamoDbLocal = builder
    .AddAWSDynamoDBLocal("DynamoDBProducts");

builder.Eventing.Subscribe<ResourceReadyEvent>(dynamoDbLocal.Resource, async (evnt, ct) =>
{
    Console.WriteLine($"Creating DynamoDB table for {evnt.Resource.Name}...");

    // Configure DynamoDB service client to connect to DynamoDB local.
    var serviceUrl = dynamoDbLocal.Resource.GetEndpoint("http").Url;
    var credentials = new BasicAWSCredentials("dummyaccesskey", "dummysecretkey");
    var ddbClient = new AmazonDynamoDBClient(new AmazonDynamoDBConfig
        { ServiceURL = serviceUrl, DefaultAWSCredentials = credentials });

    // Create the Accounts table.
    await ddbClient.CreateTableAsync(new CreateTableRequest
    {
        TableName = "Products",
        AttributeDefinitions = new List<AttributeDefinition>
        {
            new() { AttributeName = "id", AttributeType = "S" }
        },
        KeySchema = new List<KeySchemaElement>
        {
            new() { AttributeName = "id", KeyType = "HASH" }
        },
        BillingMode = BillingMode.PAY_PER_REQUEST
    }, ct);

    Console.WriteLine($"Table created");
});

var listProductsLambdaFunction = builder.AddAWSLambdaFunction<ProductAPI>("ListProductsFunction",
        "ProductAPI::ProductAPI.Api_List_Generated::List")
    .WaitFor(dynamoDbLocal)
    .WithReference(dynamoDbLocal)
    .WithEnvironment("PRODUCT_TABLE_NAME", "Products")
    .WithEnvironment("AWS_ACCESS_KEY_ID", "dummyaccesskey")
    .WithEnvironment("AWS_SECRET_ACCESS_KEY", "dummysecretaccesskey");
var getProductLambdaFunction = builder.AddAWSLambdaFunction<ProductAPI>("GetProductFunction",
        "ProductAPI::ProductAPI.Api_Get_Generated::Get")
    .WaitFor(dynamoDbLocal)
    .WithReference(dynamoDbLocal)
    .WithEnvironment("PRODUCT_TABLE_NAME", "Products")
    .WithEnvironment("AWS_ACCESS_KEY_ID", "dummyaccesskey")
    .WithEnvironment("AWS_SECRET_ACCESS_KEY", "dummysecretaccesskey");
var createProductFunction = builder.AddAWSLambdaFunction<ProductAPI>("CreateProductFunction",
        "ProductAPI::ProductAPI.Api_Create_Generated::Create")
    .WaitFor(dynamoDbLocal)
    .WithReference(dynamoDbLocal)
    .WithEnvironment("PRODUCT_TABLE_NAME", "Products")
    .WithEnvironment("AWS_ACCESS_KEY_ID", "dummyaccesskey")
    .WithEnvironment("AWS_SECRET_ACCESS_KEY", "dummysecretaccesskey");
var deleteProductFunction = builder.AddAWSLambdaFunction<ProductAPI>("DeleteProductFunction",
        "ProductAPI::ProductAPI.Api_Delete_Generated::Delete")
    .WaitFor(dynamoDbLocal)
    .WithReference(dynamoDbLocal)
    .WithEnvironment("PRODUCT_TABLE_NAME", "Products")
    .WithEnvironment("AWS_ACCESS_KEY_ID", "dummyaccesskey")
    .WithEnvironment("AWS_SECRET_ACCESS_KEY", "dummysecretaccesskey");

var lambdaServiceEmulator = builder.Resources.FirstOrDefault(resource => resource.Name == "LambdaServiceEmulator")!;

var productRestockedEventHandler = builder.AddAWSLambdaFunction<ProductAPI>("ProductRestockedEventHandler",
        "ProductAPI::ProductAPI.ProductRestockedEventHandler_Handle_Generated::Handle")
    .WaitFor(dynamoDbLocal)
    .WithReference(dynamoDbLocal)
    .WithLambdaTestCommands(lambdaServiceEmulator,
        new LambdaTestSqsMessage<ProductRestockedTestMessage>("product.restocked.v1", "ProductRestockedEventHandler",
            new ProductRestockedTestMessage("testproduct", 100)))
    .WithLambdaTestCommands(lambdaServiceEmulator,
        new LambdaTestSqsMessage<ProductRestockedTestMessage>("product.restocked.invalid.v1", "ProductRestockedEventHandler",
            new ProductRestockedTestMessage("testproduct", 100)))
    .WithEnvironment("PRODUCT_TABLE_NAME", "Products")
    .WithEnvironment("AWS_ACCESS_KEY_ID", "dummyaccesskey")
    .WithEnvironment("AWS_SECRET_ACCESS_KEY", "dummysecretaccesskey");
var productPurchasedEventHandler = builder.AddAWSLambdaFunction<ProductAPI>("ProductPurchasedEventHandler",
        "ProductAPI::ProductAPI.ProductPurchasedEventHandler_Handle_Generated::Handle")
    .WaitFor(dynamoDbLocal)
    .WithReference(dynamoDbLocal)
    .WithLambdaTestCommands(lambdaServiceEmulator,
        new LambdaTestSqsMessage<ProductPurchasedTestMessage>("product.restocked.v1", "ProductPurchasedEventHandler",
            new ProductPurchasedTestMessage("testproduct","ordernumber")))
    .WithEnvironment("PRODUCT_TABLE_NAME", "Products")
    .WithEnvironment("AWS_ACCESS_KEY_ID", "dummyaccesskey")
    .WithEnvironment("AWS_SECRET_ACCESS_KEY", "dummysecretaccesskey");

builder.AddAWSAPIGatewayEmulator("APIGatewayEmulator", APIGatewayType.HttpV2)
    .WaitFor(listProductsLambdaFunction)
    .WithReference(listProductsLambdaFunction, Method.Get, "/api/products")
    .WithReference(getProductLambdaFunction, Method.Get, "/api/products/{id}")
    .WithReference(createProductFunction, Method.Post, "/api/products")
    .WithReference(deleteProductFunction, Method.Delete, "/api/products/{id}");

await builder.Build().RunAsync();