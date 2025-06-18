using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.Core;
using AWS.Lambda.Powertools.Logging;
using ProductAPI.DataAccess;
using ProductAPI.ProductManagement;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ProductAPI;

public class Api(IProducts products)
{
    [LambdaFunction]
    [HttpApi(LambdaHttpMethod.Delete, "/api/products/{id}")]
    public async Task<IHttpResult> Delete(string id)
    {
        try
        {
            var product = await products.WithId(id);

            if (product == null) return HttpResults.NotFound("Not found");

            await products.Delete(product.Id);

            return HttpResults.Ok($"Product with id {id} deleted");
        }
        catch (ArgumentNullException e)
        {
            Logger.LogError(e, "Error deleting product with id {Id}", id);
            return HttpResults.InternalServerError("Internal error");
        }
    }

    [LambdaFunction]
    [HttpApi(LambdaHttpMethod.Post, "/api/products")]
    public async Task<IHttpResult> Create([FromBody] CreateProductRequest request)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(request, nameof(request));
            
            var product = await products.AddNew(new ProductName(request.Name), new ProductPrice(request.Price));

            return HttpResults.Created($"/products/{product.Id}");
        }
        catch (ArgumentNullException e)
        {
            Logger.LogError(e, "Error creating product");
            return HttpResults.BadRequest("Invalid request, required values are missing");
        }
    }

    [LambdaFunction]
    [HttpApi(LambdaHttpMethod.Get, "/api/products")]
    public async Task<IHttpResult> List()
    {
        var products1 = await products.All();

        return HttpResults.Ok(products1);
    }

    [LambdaFunction]
    [HttpApi(LambdaHttpMethod.Get, "/api/products/{id}")]
    public async Task<IHttpResult> Get(string id)
    {
        try
        {
            var product = await products.WithId(id);

            if (product == null) return HttpResults.NotFound("Not found");

            return HttpResults.Ok(product);
        }
        catch (ArgumentNullException e)
        {
            Logger.LogError(e, "Error retrieving product with id {Id}", id);
            return HttpResults.InternalServerError("Internal error");
        }
    }
}