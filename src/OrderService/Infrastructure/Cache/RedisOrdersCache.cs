using System.Text.Json;
using StackExchange.Redis;
using Order = OrderService.Domain.Entities.Order;

namespace OrderService.Infrastructure.Cache;

public sealed class RedisOrdersCache : IOrdersCache
{
    private static readonly TimeSpan CacheTtl       = TimeSpan.FromMinutes(5);
    private static readonly EventId  CacheHitEvent  = new(1001, nameof(CacheHitEvent));
    private static readonly EventId  CacheMissEvent = new(1002, nameof(CacheMissEvent));

    private readonly IDatabase                 _database;
    private readonly ILogger<RedisOrdersCache> _logger;

    public RedisOrdersCache(IConnectionMultiplexer connectionMultiplexer, ILogger<RedisOrdersCache> logger)
    {
        _database = connectionMultiplexer.GetDatabase();
        _logger = logger;
    }

    public async Task<Order?> GetAsync(Guid id)
    {
        var value = await _database.StringGetAsync(GetKey(id));

        if (value.IsNullOrEmpty)
        {
            _logger.LogInformation(CacheMissEvent, $"Redis cache miss for order {id}");
            return null;
        }

        _logger.LogInformation(CacheHitEvent, $"Redis cache hit for order {id}");

        return JsonSerializer.Deserialize<Order>(value!.ToString());
    }

    public async Task SetAsync(Order order)
    {
        var cacheKey = GetKey(order.Id);

        var value = JsonSerializer.Serialize(order);

        await _database.StringSetAsync(cacheKey, value, CacheTtl);

        _logger.LogInformation($"Order {order.Id} cached in Redis");
    }

    public async Task RemoveAsync(Guid id)
    {
        await _database.KeyDeleteAsync(GetKey(id));

        _logger.LogInformation($"Redis cache invalidated for order {id}");
    }

    private string GetKey(Guid id)
    {
        return $"orders:{id}";
    }
}