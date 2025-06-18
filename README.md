# Serverless Aspire

Did you know you can use .NET Aspire when developing serverless applications? You didn't? Well aren't you in luck! This repository demonstrates how you can dramatically improve your local developer experience, whether you're using AWS Lambda or Azure Functions, to locally run and debug serverless applications.

## Running Locally

The repository is split down into two different folders, for AWS and Azure respectively. Inside each folder there is a the application code, a test project and the .NET Aspire project. To run each application locally:

### AWS

The AWS example uses the Lambda and API Gateway emulation support [built by the .NET team at AWS](https://aws.amazon.com/blogs/developer/building-lambda-with-aspire-part-1/). It allows you to startup [DynamoDB Local](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/DynamoDBLocal.html), all of your Lambda functions and an API Gateway endpoint that routes inbound HTTP requests to the correct function. It also starts up one additional function to simulate how you could test a Lambda function sourced by Amazon SQS.

```sh
cd AWS/Serverless.Aspire.AWS
dotnet run
```

### Azure

The Azure example uses the [Azure functions support for .NET Aspire](https://learn.microsoft.com/en-us/dotnet/aspire/serverless/functions?tabs=dotnet-cli&pivots=dotnet-cli). It allows you to startup your Azure functions locally as well as a HTTP endpoint routes inbound requests to the correct function.

```sh
cd Azure/Serverless.Aspire.Azure
dotnet run
```

You can also run the application inside your IDE of choice, in **Debug** mode, to get the ability to step through your function code when invoked.

## Testing

Each of the cloud provider specific repository contains a test project which uses the [.NET Aspire Test](https://learn.microsoft.com/en-us/dotnet/aspire/testing/write-your-first-test?pivots=xunit) libraries to automatically startup your functions and their dependencies before sending in test requests. This uses the `TestFixture` support in xUnit to run the setup before any tests run.

```csharp

public ApiDriver ApiDriver { get; init; }

public TestSetupFixture()
{
    var builder = DistributedApplicationTestingBuilder
        .CreateAsync<Projects.Serverless_Aspire_AWS>()
        .GetAwaiter()
        .GetResult();
    
    App = builder.BuildAsync().GetAwaiter().GetResult();

    App.StartAsync().GetAwaiter().GetResult();
    
    using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(30));
    App.ResourceNotifications.WaitForResourceHealthyAsync(
            "APIGatewayEmulator",
            cts.Token)
        .GetAwaiter().GetResult();
    
    var httpClient = App.CreateHttpClient("APIGatewayEmulator", "http");
    
    var lambdaTestToolUrl = App.GetEndpoint("LambdaServiceEmulator", "http");
    ApiDriver = new ApiDriver(httpClient, new AmazonLambdaClient(new AmazonLambdaConfig()
    {
        ServiceURL = lambdaTestToolUrl.ToString(),
        DefaultAWSCredentials = new Amazon.Runtime.BasicAWSCredentials("dumykey", "dummysecret")
    }));
}
```

The `TestSetupFixture` class is then injected into the test classes, providing access to the `ApiDriver` class.

```csharp
public class ApiTests(TestSetupFixture setupFixture) : IClassFixture<TestSetupFixture>
{   
    [Fact]
    public async Task CanCreateNewProductShouldReturn201()
    {
        var productName = "Test Product";
        var result = await setupFixture.ApiDriver.PostProductAsync(productName, 10);
        
        result.EnsureSuccessStatusCode();
        Assert.Equal(201, (int)result.StatusCode);
    }
}
```

### Running Tests

You can run the `dotnet test` command to quickly run the tests for your cloud provider of choice. However, if you run the tests in debug mode *inside your IDE* that will also start up .NET Aspire and all your funciton code in debug mode as well. This allows you to step through your function code as part of the test run.