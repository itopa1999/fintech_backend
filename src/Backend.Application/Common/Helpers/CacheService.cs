using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Backend.Application.Common.Helpers;

public class CacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CacheService> _logger;
    private readonly JsonSerializerOptions _serializerOptions;

    public CacheService(IDistributedCache cache, ILogger<CacheService> logger, JsonSerializerOptions? serializerOptions = null)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serializerOptions = serializerOptions ?? new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Cache key must not be empty.", nameof(key));
        }

        _logger.LogInformation("RedisKey: {redisKey}", key);

        var cachedString = await _cache.GetStringAsync(key, cancellationToken);
        if (string.IsNullOrEmpty(cachedString))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(cachedString, _serializerOptions);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpirationRelativeToNow = null, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Cache key must not be empty.", nameof(key));
        }

        _logger.LogInformation("RedisKey: {redisKey}", key);

        var json = JsonSerializer.Serialize(value, _serializerOptions);
        var cacheOptions = new DistributedCacheEntryOptions();

        if (absoluteExpirationRelativeToNow.HasValue)
        {
            cacheOptions.SetAbsoluteExpiration(absoluteExpirationRelativeToNow.Value);
        }

        if (slidingExpiration.HasValue)
        {
            cacheOptions.SetSlidingExpiration(slidingExpiration.Value);
        }

        await _cache.SetStringAsync(key, json, cacheOptions, cancellationToken);
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Cache key must not be empty.", nameof(key));
        }

        _logger.LogInformation("RedisKey: {redisKey}", key);
        return _cache.RemoveAsync(key, cancellationToken);
    }

    public async Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? absoluteExpirationRelativeToNow = null, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default)
    {
        if (factory is null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Cache key must not be empty.", nameof(key));
        }

        _logger.LogInformation("RedisKey: {redisKey}", key);

        var existingValue = await GetAsync<T>(key, cancellationToken);
        if (existingValue is not null)
        {
            return existingValue;
        }

        var createdValue = await factory();
        if (createdValue is null)
        {
            return default;
        }

        await SetAsync(key, createdValue, absoluteExpirationRelativeToNow, slidingExpiration, cancellationToken);
        return createdValue;
    }
}
