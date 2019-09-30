# azure-servicebus-polly

[![Build Status](https://dev.azure.com/jpgoncalves/GH%20Projects/_apis/build/status/goncalvesj.azure-servicebus-polly?branchName=master)](https://dev.azure.com/jpgoncalves/GH%20Projects/_build/latest?definitionId=10&branchName=master)

.NET Standard dll that send messages to a service bus namespace with Polly policies for retry and fallback

In summary the nuget package performs the following:

1. Sets Up primary Service Bus connection.
2. Sets Up Secondary Service Bus connection if secondary connection string exists.
   (Only used in the Live environment)
3. Sets Up Retry and Fallback policies if secondary connection string exists.
   (Default Retry Count = 3, Default Retry Seconds = 3)
4. Logs Exception in case of failure in secondary namespace.
   (Error message is logged in an Error Queue, Default Queue Name = Error)

This package is composed by:

## IBaseMessage interface

```c#
public interface IBaseMessage
{
    string Label { get; set; }
}
```

## IMessaging interface

```c#
public interface IMessaging
{
    /// <summary>
    ///     Sends a message of type IBaseMessage to a Service Bus Topic or Queue
    ///<para />
    ///     Message is Encoded in UTF8 and Serialized to Json before being sent
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="messageToSend"></param>
    /// <returns></returns>
    Task SendMessageAsync<T>(T messageToSend) where T : IBaseMessage;
}
```

## Messaging Option

```c#
public class MessagingOptions
{
    public string TopicName { get; set; }

    public string ErrorQueueName { get; set; } = Globals.ErrorQueueName;

    public int RetryCount { get; set; } = Globals.RetryCount;

    public int RetrySeconds { get; set; } = Globals.RetrySeconds;

    public string ServiceBusConnectionString { get; set; }

    public string ServiceBusConnectionStringSecondary { get; set; } = string.Empty;
}
```

## Usage steps

Set up package in Startup.cs Needs Topic Name, Primary Connection String and Secondary Connection String. Secondary Connections String and Error Queue is used only in the Live environment, for all other environments the value can be empty.

Get MessagingOptions properties from app.settings.json.  
Add IMessaging service.  
Messaging options are inherited automatically through dependency injection.

```c#
services.Configure<MessagingOptions>(options =>
{
    configuration.GetSection("MessagingOptions").Bind(options);
});

services.AddScoped<IMessaging, Messaging>();
```

_**DTO inherits from IBaseMessage.**_

```c#
public class TestDto: IBaseMessage
{
    public string Label { get; set; }
    public string Message { get; set; }
}
```

Below are two examples on how to send messages to the Service Bus. The first is usually implemented directly from a Controller, the second example allows this to be accessed from anywhere , typically a Service.

## Example: Command

_**Example class to send a message to the Service Bus. Any DTO can be sent as long as it inherits from the Base Message interface. (The below code uses the Mediatr nuget package)**_

```c#
namespace Commands.Test
{
    public class SendMessageToServiceBusCommand : IRequest
    {
        public IBaseMessage ServiceBusMessage { get; set; }
    }

    public class SendMessageToServiceBusCommandHandler : IRequestHandler<SendMessageToServiceBusCommand>
    {
        private readonly IMessaging _messaging;

        public SendMessageToServiceBusCommandHandler(IMessaging messaging)
        {
            _messaging = messaging;
        }

        public async Task<Unit> Handle(SendMessageToServiceBusCommand request)
        {
            await _messaging.SendMessageAsync(request.ServiceBusMessage);

            return Unit.Value;
        }
    }
}
```

## Example: Controller

```c#
[HttpPost]
public async Task<IActionResult> SendMessage([FromBody] TestDto model)
{
    if (model == null)
        return BadRequest();

    var command = new SendMessageToServiceBusCommand
    {
        ServiceBusMessage = model
    }

    await Mediator.Send(command);

    return Ok();
}
```
