using OrderService.Application.DTOs;
using OrderService.Domain.Entities;

namespace OrderService.Application.Services.OrdersService;

public interface IOrdersService
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken = default);
    
    Task<Order> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);
    
    Task<Order?> UpdateStatusAsync(Guid id, UpdateOrderStatusRequest request, CancellationToken cancellationToken = default);
    
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}