using OrderService.Domain.Entities;

namespace OrderService.Application.Services.OrdersCache;

public interface IOrdersCache
{
    Task<Order?> GetAsync(Guid id);
    Task SetAsync(Order order);
    Task RemoveAsync(Guid id);
}