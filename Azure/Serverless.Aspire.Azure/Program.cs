using Projects;

#pragma warning disable CA2252 // Opt in to preview features

var builder = DistributedApplication.CreateBuilder(args);

var db = builder
    .AddPostgres("database")
    .WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("products");

var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator();

var functions = builder.AddAzureFunctionsProject<ProductAPI_AzureFunctions>("functions")
    .WaitFor(db)
    .WithHostStorage(storage)
    .WithEnvironment("CONNECTION_STRING", db)
    .WithExternalHttpEndpoints();

builder.AddProject<ProductAPI_AzureFunctions>("ProductAPI")
    .WithEnvironment("CONNECTION_STRING", db)
    .WithReference(functions)
    .WaitFor(functions);

await builder.Build().RunAsync();