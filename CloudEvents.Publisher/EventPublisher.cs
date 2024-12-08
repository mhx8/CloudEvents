using Azure.Messaging;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Azure;

namespace CloudEvents.Publisher;

public class EventPublisher(
    IAzureClientFactory<ServiceBusClient> clientFactory,
    IConfiguration configuration)
{
    public async Task SendCloudEventAsync(
        CloudEvent cloudEvent)
    {
        ServiceBusClient? client = clientFactory.CreateClient("Default");
        ServiceBusSender? sender = client.CreateSender(configuration["AzureServiceBus:TopicName"]);

        ServiceBusMessage message = new(new BinaryData(cloudEvent))
        {
            ContentType = "application/cloudevents+json"
        };

        await sender.SendMessageAsync(message);
    }
}