using Microsoft.EntityFrameworkCore;
using Orders.Contracts.Events;
using OrderService.Application.DTOs;
using OrderService.Infrastructure.Cache;
using OrderService.Infrastructure.Persistence;
using Shared.Messaging.Abstractions;
using Order = OrderService.Domain.Entities.Order;

namespace OrderService.Application.Services.OrdersService;

public sealed class OrdersService : IOrdersService
{
    private readonly OrdersDbContext _dbContext;
    private readonly IOrdersCache    _ordersCache;
    private readonly IEventPublisher _eventPublisher;

    public OrdersService(OrdersDbContext dbContext, IOrdersCache ordersCache, IEventPublisher eventPublisher)
    {
        _dbContext = dbContext;
        _ordersCache = ordersCache;
        _eventPublisher = eventPublisher;
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
            UserName = request.UserName,
            ProductName = request.ProductName,
            Quantity = request.Quantity,
            Status = "Created",
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var orderCreatedEvent = new OrderCreatedEvent(
            order.Id,
            order.UserName,
            order.ProductName,
            order.Quantity,
            order.CreatedAt
        );

        await _eventPublisher.PublishAsync(orderCreatedEvent, "order.created", cancellationToken);

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

        await _ordersCache.RemoveAsync(id);

        return order;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _dbContext.Orders.FirstOrDefaultAsync(order => order.Id == id, cancellationToken);

        if (order is null)
            return false;

        _dbContext.Orders.Remove(order);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _ordersCache.RemoveAsync(id);

        return true;
    }
}