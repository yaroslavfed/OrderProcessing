using Microsoft.EntityFrameworkCore;
using OrderService.Application.DTOs;
using OrderService.Application.Services.OrdersCache;
using OrderService.Infrastructure.Persistence;
using Order = OrderService.Domain.Entities.Order;

namespace OrderService.Application.Services.OrdersService;

public sealed class OrdersService : IOrdersService
{
    private readonly OrdersDbContext _dbContext;
    private readonly IOrdersCache     _ordersCache;

    public OrdersService(OrdersDbContext dbContext, IOrdersCache ordersCache)
    {
        _dbContext = dbContext;
        _ordersCache = ordersCache;
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cachedOrder = await _ordersCache.GetAsync(id);

        if (cachedOrder is not null)
            return cachedOrder;

        var order = await _dbContext
                          .Orders.AsNoTracking()
                          .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);

        if (order is null)
            return null;

        await _ordersCache.SetAsync(order);
        return order;
    }

    public async Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext
                     .Orders.AsNoTracking()
                     .OrderByDescending(order => order.CreatedAt)
                     .ToListAsync(cancellationToken);
    }

    public async Task<Order> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerName = request.CustomerName,
            TotalAmount = request.TotalAmount,
            Status = "Created",
            CreatedAt = DateTime.UtcNow,
        };

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return order;
    }

    public async Task<Order?> UpdateStatusAsync(
        Guid id,
        UpdateOrderStatusRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var order = await _dbContext.Orders.FirstOrDefaultAsync(order => order.Id == id, cancellationToken);

        if (order is null)
            return null;

        order.Status = request.Status;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return order;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _dbContext.Orders.FirstOrDefaultAsync(order => order.Id == id, cancellationToken);

        if (order is null)
            return false;

        _dbContext.Orders.Remove(order);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}