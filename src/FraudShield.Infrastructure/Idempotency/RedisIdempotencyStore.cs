using System.Text.Json;
using FraudShield.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace FraudShield.Infrastructure.Idempotency;

public class RedisIdempotencyStore : IIdempotencyStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IDistributedCache _cache;
    public RedisIdempotencyStore(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<IdempotencyEntry?> GetAsync(string key)
    {
        var json = await _cache.GetStringAsync(key);
        if (string.IsNullOrWhiteSpace(json))
            return null;

        return JsonSerializer.Deserialize<IdempotencyEntry>(json, JsonOptions);
    }

    public async Task SaveAsync(string key, IdempotencyEntry entry)
    {
        var json = JsonSerializer.Serialize(entry, JsonOptions);

        await _cache.SetStringAsync(
            key,
            json,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
            });
    }
}

