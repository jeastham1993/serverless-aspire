// Unless explicitly stated otherwise all files in this repository are licensed under the Apache License Version 2.0.
// This product includes software developed at Datadog (https://www.datadoghq.com/).
// Copyright 2025 Datadog, Inc.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using ProductAPI.AzureFunctions.DataAccess;
using ProductAPI.AzureFunctions.ProductManagement;

namespace ProductAPI.AzureFunctions;

public class Api(IProducts products, ProductDbContext dbContext)
{
    [Function("Health")]
    public async Task<IActionResult> HealthCheck(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")]
        HttpRequest req)
    {
        await dbContext.Database.EnsureCreatedAsync();

        return new OkObjectResult("OK");
    }

    [Function("ListProducts")]
    public async Task<IActionResult> ListProducts(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products")]
        HttpRequest req)
    {
        var productList = await products.All();

        return new OkObjectResult(productList);
    }

    [Function("GetProduct")]
    public async Task<IActionResult> GetProduct(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products/{id}")]
        HttpRequest req)
    {
        var id = req.RouteValues["id"]?.ToString() ?? throw new ArgumentException("Product ID is required.");

        var product = await products.WithId(id);

        return new OkObjectResult(product);
    }

    [Function("CreateProduct")]
    public async Task<IActionResult> CreateProduct(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "products")]
        HttpRequest req)
    {
        var createProduct = await req.ReadFromJsonAsync<CreateProductRequest>() ??
                            throw new ArgumentException("Invalid product data.");

        var newProduct =
            await products.New(new ProductName(createProduct.Name), new ProductPrice(createProduct.Price));

        return new CreatedResult($"/api/products/${newProduct.Id}", newProduct);
    }

    [Function("DeleteProduct")]
    public async Task<IActionResult> DeleteProduct(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "products/{id}")]
        HttpRequest req)
    {
        var id = req.RouteValues["id"]?.ToString() ?? throw new ArgumentException("Product ID is required.");

        await products.Delete(id);

        return new OkObjectResult("Deleted");
    }
}