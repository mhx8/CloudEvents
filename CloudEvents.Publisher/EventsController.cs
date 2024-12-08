using System.Text.Json;
using Azure.Messaging;
using CloudEvents.Shared;
using Microsoft.AspNetCore.Mvc;

namespace CloudEvents.Publisher;

[ApiController]
[Route("api/events")]
public class EventsController(
    EventPublisher eventPublisher,
    ILogger<EventsController> logger) : ControllerBase
{
    [HttpGet("{message}")]
    public async Task<IActionResult> SendEvent(
        string message)
    {
        CloudEvent cloudEvent = new(
            source: "https://mhx8.com/events",
            type: MessageContract.EventDataMessageType,
            jsonSerializableData: new EventData(message)
        );
        
        CloudEvent cloudEventNew = new(
            source: "https://mhx8.com/events",
            type: MessageContract.EventDataMessageTypeV2,
            jsonSerializableData: new EventDataV2(message)
        );
        
        await eventPublisher.SendCloudEventAsync(cloudEvent);
        await eventPublisher.SendCloudEventAsync(cloudEventNew);

        logger.LogInformation(JsonSerializer.Serialize(cloudEvent));

        return Ok("Event sent! 🚀");
    }
}