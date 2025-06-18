#pragma warning disable CA1822 // Non-static required by Lambda Annotations

using Amazon.Lambda.Annotations;
using Amazon.Lambda.SQSEvents;

namespace ProductAPI;

public class MessageHandlers
{
    [LambdaFunction]
    public void HandleSqsMessage(SQSEvent evt)
    {
        ArgumentNullException.ThrowIfNull(evt, nameof(evt));
        
        foreach (var message in evt.Records)
        {
            // Process each message
            Console.WriteLine($"Received message: {message.Body}");
            
            // Here you can add logic to process the message, e.g., save to a database, etc.
            // For demonstration, we just log the message body.
        }
    }
}