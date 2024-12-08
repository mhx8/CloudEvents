using Microsoft.Extensions.Caching.Distributed;

namespace CloudEvents.Consumer;

public class IdempotencyChecker(
    IDistributedCache cache)
{
    public async Task<bool> IsDuplicateAsync(string eventId)
    {
        string? cachedValue = await cache.GetStringAsync(eventId);

        if (cachedValue != null)
        {
            return true;
        }

        await cache.SetStringAsync(eventId, "processed");
        return false;
    }
}