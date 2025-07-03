using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Aspire.Hosting.AWS.DynamoDB;
using Aspire.Hosting.AWS.Lambda;
using Projects;
using Serverless.Aspire.AWS;
using Serverless.Aspire.AWS.DatadogAspireExtensions;
using Serverless.Aspire.AWS.Localstack;

#pragma warning disable CA2252 // Opt in to preview features

var builder = DistributedApplication.CreateBuilder(args);

var options = new DatadogOptionsBuilder(builder.Configuration["DDApiKey"]!)
    .WithDDSite("datadoghq.eu")
    .WithServiceName("ProductAPI")
    .WithAPM()
    .WithLogs()
    .WithDogStatsD()
    .WithProfiling()
    .WithRuntimeMetrics()
    .WithOTELEndpoints()
    .Build();

var datadogContainer = builder.AddDatadog(options);

var localStackOptions = builder.AddLocalStackConfig();

var localStack = builder
    .AddLocalStack("localstack", localStackOptions);

var dynamoDbLocal = builder
    .AddAWSDynamoDBLocal("DynamoDBProducts");

var cdkBootstrapTemplate = Path.Combine(Directory.GetCurrentDirectory(), "cdk-bootstrap.template");

if (!File.Exists(cdkBootstrapTemplate))
{
    throw new ArgumentException($"Could not find `{cdkBootstrapTemplate}` template.");
}

// Bootstrap the CDK environment against LocalStack.
var cdkBootstrap = builder.AddAWSCloudFormationTemplate("CDKBootstrap", cdkBootstrapTemplate)
    .WaitFor(localStack)
    .WithLocalStack(localStackOptions);

var cdkStack = builder.AddAWSCDKStack("ProductCDKStack")
    .WaitFor(localStack)
    .WaitFor(cdkBootstrap)
    .WithLocalStack(localStackOptions);
var createdTopic = cdkStack.AddSNSTopic("ProductCreatedTopic")
    .AddOutput("ProductCreatedTopicArn", construct => construct.TopicArn);
var deletedTopic = cdkStack.AddSNSTopic("ProductDeletedTopic")
    .AddOutput("ProductDeletedTopicArn", construct => construct.TopicArn);

// Uncomment the following lines if you want to use a CloudFormation template for AWS resources.
// var awsResources = builder.AddAWSCloudFormationTemplate("AWSResources", "aws-resources.template")
//     .WaitFor(localStack)
//     .WithLocalStack(localStackOptions);

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

var lambdaCommonReferences = new LambdaCommonReferences(dynamoDbLocal, localStack);

var listProductsLambdaFunction = builder.AddAWSLambdaFunction<ProductAPI>("ListProductsFunction",
        "ProductAPI::ProductAPI.Api_List_Generated::List")
    .WithCommonReferences(lambdaCommonReferences)
    .WithEnvironment("DD_TRACE_AGENT_PORT", "8126")
    .WithDatadog(options);

var getProductLambdaFunction = builder.AddAWSLambdaFunction<ProductAPI>("GetProductFunction",
        "ProductAPI::ProductAPI.Api_Get_Generated::Get")
    .WithCommonReferences(lambdaCommonReferences)
    .WithDatadog(options);
var createProductFunction = builder.AddAWSLambdaFunction<ProductAPI>("CreateProductFunction",
        "ProductAPI::ProductAPI.Api_Create_Generated::Create")
    .WithCommonReferences(lambdaCommonReferences)
    // Example of using CloudFormation resource instead of LocalStack resource.
    //.WithEnvironment("PRODUCT_CREATED_TOPIC_ARN", awsResources.ExtractOutputValueFor("ProductCreatedTopicArn"))
    .WithReference(createdTopic)
    .WithEnvironment("PRODUCT_CREATED_TOPIC_ARN", cdkStack.ExtractOutputValueFor("ProductCreatedTopicArn"))
    .WithDatadog(options);
var deleteProductFunction = builder.AddAWSLambdaFunction<ProductAPI>("DeleteProductFunction",
        "ProductAPI::ProductAPI.Api_Delete_Generated::Delete")
    .WithCommonReferences(lambdaCommonReferences)
    .WithReference(deletedTopic)
    .WithEnvironment("PRODUCT_DELETED_TOPIC_ARN", cdkStack.ExtractOutputValueFor("ProductDeletedTopicArn"))
    .WithDatadog(options);

var lambdaServiceEmulator = builder.Resources.FirstOrDefault(resource => resource.Name == "LambdaServiceEmulator")!;

var productRestockedEventHandler = builder.AddAWSLambdaFunction<ProductAPI>("ProductRestockedEventHandler",
        "ProductAPI::ProductAPI.ProductRestockedEventHandler_Handle_Generated::Handle")
    .WaitFor(dynamoDbLocal)
    .WithReference(dynamoDbLocal)
    // If you only have a single Lambda function in your project, you'll need to first create the lambda handler
    // Then retrieve the LambdaServiceEmulator resource from the builder
    // Then add the test commands passing in the emulator.
    .WithLambdaTestCommands(lambdaServiceEmulator,
        new LambdaTestSqsMessage<ProductRestockedTestMessage>("product.restocked.v1", "ProductRestockedEventHandler",
            new ProductRestockedTestMessage("testproduct", 100)))
    .WithLambdaTestCommands(lambdaServiceEmulator,
        new LambdaTestSqsMessage<ProductRestockedTestMessage>("product.restocked.invalid.v1",
            "ProductRestockedEventHandler",
            new ProductRestockedTestMessage(null, 100)))
    .WithEnvironment("PRODUCT_TABLE_NAME", "Products")
    .WithEnvironment("AWS_ACCESS_KEY_ID", "dummyaccesskey")
    .WithEnvironment("AWS_SECRET_ACCESS_KEY", "dummysecretaccesskey")
    .WithDatadog(options);
var productPurchasedEventHandler = builder.AddAWSLambdaFunction<ProductAPI>("ProductPurchasedEventHandler",
        "ProductAPI::ProductAPI.ProductPurchasedEventHandler_Handle_Generated::Handle")
    .WaitFor(dynamoDbLocal)
    .WithReference(dynamoDbLocal)
    .WithLambdaTestCommands(lambdaServiceEmulator,
        new LambdaTestSqsMessage<ProductPurchasedTestMessage>("product.restocked.v1", "ProductPurchasedEventHandler",
            new ProductPurchasedTestMessage("testproduct", "ordernumber")))
    .WithEnvironment("PRODUCT_TABLE_NAME", "Products")
    .WithEnvironment("AWS_ACCESS_KEY_ID", "dummyaccesskey")
    .WithEnvironment("AWS_SECRET_ACCESS_KEY", "dummysecretaccesskey")
    .WithDatadog(options);

builder.AddAWSAPIGatewayEmulator("APIGatewayEmulator", APIGatewayType.HttpV2)
    .WaitFor(listProductsLambdaFunction)
    .WithReference(listProductsLambdaFunction, Method.Get, "/api/products")
    .WithReference(getProductLambdaFunction, Method.Get, "/api/products/{id}")
    .WithReference(createProductFunction, Method.Post, "/api/products")
    .WithReference(deleteProductFunction, Method.Delete, "/api/products/{id}");

await builder.Build().RunAsync();