using System.Text.Json;
using Azure.Messaging;
using Azure.Messaging.ServiceBus;
using CloudEvents.Shared;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CloudEvents.Consumer;

public class EventConsumer : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IdempotencyChecker _idempotencyChecker;
    private readonly ServiceBusProcessor _processor;

    public EventConsumer(
        IAzureClientFactory<ServiceBusClient> clientFactory,
        ILogger<EventConsumer> logger,
        IConfiguration configuration,
        IdempotencyChecker idempotencyChecker)
    {
        _logger = logger;
        _idempotencyChecker = idempotencyChecker;
        ServiceBusClient? client = clientFactory.CreateClient("Default");
        _processor = client.CreateProcessor(
            configuration["AzureServiceBus:TopicName"],
            "ConsumerSubscription",
            new ServiceBusProcessorOptions());
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _processor.ProcessMessageAsync += ProcessMessageAsync;
        _processor.ProcessErrorAsync += ProcessErrorAsync;
        await _processor.StartProcessingAsync(stoppingToken);
    }

    private async Task ProcessMessageAsync(
        ProcessMessageEventArgs args)
    {
        CloudEvent cloudEvent = CloudEvent.Parse(args.Message.Body)!;
        
        if (await _idempotencyChecker.IsDuplicateAsync(cloudEvent.Id))
        {
            _logger.LogInformation($"Duplicate event: {cloudEvent.Id}");
            await args.CompleteMessageAsync(args.Message);
            return;
        }
        
        // Handle V2
        if (cloudEvent.Type == MessageContract.EventDataMessageTypeV2)
        { 
            EventDataV2 deserializedData = cloudEvent.Data!.ToObjectFromJson<EventDataV2>()!;
            _logger.LogInformation(
                $"Received CloudEvent: Id={cloudEvent.Id}, Type={cloudEvent.Type}, Message={deserializedData.Msg}");
        }

        await args.CompleteMessageAsync(args.Message);
    }

    private Task ProcessErrorAsync(
        ProcessErrorEventArgs args)
    {
        _logger.LogError($"Error: {args.Exception.Message}");
        return Task.CompletedTask;
    }

    public override async Task StopAsync(
        CancellationToken cancellationToken)
    {
        await _processor.CloseAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }
}