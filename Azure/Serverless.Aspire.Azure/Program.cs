using System.Text.Json;
using Projects;
using Serverless.Aspire.Azure;

#pragma warning disable CA2252 // Opt in to preview features

var builder = DistributedApplication.CreateBuilder(args);

var db = builder
    .AddPostgres("database")
    .WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("products");

var serviceBus = builder.AddAzureServiceBus("messaging")
    .RunAsEmulator(c =>
    {
        c.WithLifetime(ContainerLifetime.Persistent);
        c.WithBindMount("servicebus-data", "/var/opt/mssql/data");
        c.WithHostPort(60001);
    });

serviceBus
    .AddServiceBusQueue("product-purchased", "products.productPurchased.v1")
    .WithServiceBusTestCommand(new ServiceBusTestCommand("Product Purchased", JsonSerializer.Serialize(new
    {
        productId = "TEST-PRODUCT",
        orderNumber = "ORD-67890",
    })));

serviceBus
    .AddServiceBusQueue("product-restocked", "products.productReStocked.v1")
    .WithServiceBusTestCommand(new ServiceBusTestCommand("Product Restocked", JsonSerializer.Serialize(new
    {
        productId = "TEST-PRODUCT",
        newStockLevel = 10,
    })));

var storage = builder
    .AddAzureStorage("storage")
    .RunAsEmulator();

var functions = builder.AddAzureFunctionsProject<ProductAPI_AzureFunctions>("functions")
    .WaitFor(db)
    .WaitFor(serviceBus)
    .WithHostStorage(storage)
    .WithEnvironment("CONNECTION_STRING", db)
    .WithEnvironment("SERVICE_BUS_CONNECTION_STRING", serviceBus)
    .WithExternalHttpEndpoints();

builder.AddProject<ProductAPI_AzureFunctions>("ProductAPI")
    .WithEnvironment("CONNECTION_STRING", db)
    .WithEnvironment("SERVICE_BUS_CONNECTION_STRING", serviceBus)
    .WithReference(functions)
    .WaitFor(functions);

await builder.Build().RunAsync();