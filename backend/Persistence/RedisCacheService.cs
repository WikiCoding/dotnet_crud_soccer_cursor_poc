using System.Text.Json;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using StackExchange.Redis;

namespace cursor_dotnet_test.Persistence;

public class RedisCacheService : IRedisCacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly ResiliencePipeline _circuitBreaker;
    private readonly TimeSpan _defaultTtl;

    public RedisCacheService(IConnectionMultiplexer redis, IOptions<RedisCacheSettings> settings, ILogger<RedisCacheService> logger)
    {
        _redis = redis;
        _logger = logger;

        var config = settings.Value;
        _defaultTtl = TimeSpan.FromSeconds(config.DefaultTtlSeconds);
        var cb = config.CircuitBreaker;

        _circuitBreaker = new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = cb.FailureRatio,
                SamplingDuration = TimeSpan.FromSeconds(cb.SamplingDurationSeconds),
                MinimumThroughput = cb.MinimumThroughput,
                BreakDuration = TimeSpan.FromSeconds(cb.BreakDurationSeconds),
                ShouldHandle = new PredicateBuilder()
                    .Handle<RedisConnectionException>()
                    .Handle<RedisTimeoutException>()
                    .Handle<RedisException>(),
                OnOpened = args =>
                {
                    logger.LogWarning("Redis circuit breaker OPENED for {BreakDuration} — skipping cache calls", args.BreakDuration);
                    return ValueTask.CompletedTask;
                },
                OnClosed = _ =>
                {
                    logger.LogInformation("Redis circuit breaker CLOSED — cache calls resumed");
                    return ValueTask.CompletedTask;
                },
                OnHalfOpened = _ =>
                {
                    logger.LogInformation("Redis circuit breaker HALF-OPEN — testing cache connectivity");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            return await _circuitBreaker.ExecuteAsync(async ct =>
            {
                var db = _redis.GetDatabase();
                var value = await db.StringGetAsync(key);

                if (value.IsNullOrEmpty)
                    return default;

                _logger.LogDebug("Cache HIT for key {Key}", key);
                return JsonSerializer.Deserialize<T>(value!);
            }, CancellationToken.None);
        }
        catch (BrokenCircuitException)
        {
            _logger.LogDebug("Cache GET skipped (circuit open) for key {Key}", key);
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis GET failed for key {Key}, falling back to database", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        try
        {
            await _circuitBreaker.ExecuteAsync(async ct =>
            {
                var db = _redis.GetDatabase();
                var serialized = JsonSerializer.Serialize(value);
                await db.StringSetAsync(key, serialized, expiry ?? _defaultTtl);
                _logger.LogDebug("Cache SET for key {Key}", key);
            }, CancellationToken.None);
        }
        catch (BrokenCircuitException)
        {
            _logger.LogDebug("Cache SET skipped (circuit open) for key {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis SET failed for key {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _circuitBreaker.ExecuteAsync(async ct =>
            {
                var db = _redis.GetDatabase();
                await db.KeyDeleteAsync(key);
                _logger.LogDebug("Cache REMOVE for key {Key}", key);
            }, CancellationToken.None);
        }
        catch (BrokenCircuitException)
        {
            _logger.LogDebug("Cache REMOVE skipped (circuit open) for key {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis REMOVE failed for key {Key}", key);
        }
    }

    public async Task RemoveByPrefixAsync(string prefix)
    {
        try
        {
            await _circuitBreaker.ExecuteAsync(async ct =>
            {
                foreach (var endpoint in _redis.GetEndPoints())
                {
                    var server = _redis.GetServer(endpoint);
                    var keys = server.Keys(pattern: $"{prefix}*").ToArray();

                    if (keys.Length == 0)
                        return;

                    var db = _redis.GetDatabase();
                    await db.KeyDeleteAsync(keys);
                    _logger.LogDebug("Cache REMOVE {Count} keys with prefix {Prefix}", keys.Length, prefix);
                }
            }, CancellationToken.None);
        }
        catch (BrokenCircuitException)
        {
            _logger.LogDebug("Cache REMOVE by prefix skipped (circuit open) for {Prefix}", prefix);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis REMOVE by prefix failed for {Prefix}", prefix);
        }
    }
}
