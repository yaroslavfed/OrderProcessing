using Microsoft.AspNetCore.Http.HttpResults;
using OrderService.Application.DTOs;
using OrderService.Application.Services.OrdersService;
using OrderService.Domain.Entities;

namespace OrderService.Application.Endpoints;

public static class OrdersEndpoints
{
    public static void MapOrdersEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var builder = endpoints.MapGroup("/orders");

        builder.MapGet("/", GetAllOrders);
        builder.MapGet("/{id}", GetOrder);
        builder.MapPost("/", CreateOrder);
        builder.MapPut("/{id}/status", UpdateOrderStatus);
        builder.MapDelete("/{id}", DeleteOrder);
    }

    private static async Task<Ok<IReadOnlyList<Order>>> GetAllOrders(
        IOrdersService ordersService,
        CancellationToken cancellationToken
    )
    {
        var orders = await ordersService.GetAllAsync(cancellationToken);

        return TypedResults.Ok(orders);
    }

    private static async Task<Results<Ok<Order>, NotFound>> GetOrder(
        Guid id,
        IOrdersService ordersService,
        CancellationToken cancellationToken
    )
    {
        var order = await ordersService.GetByIdAsync(id, cancellationToken);

        return order is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(order);
    }

    private static async Task<Created<Order>> CreateOrder(
        CreateOrderRequest request,
        IOrdersService ordersService,
        CancellationToken cancellationToken
    )
    {
        var order = await ordersService.CreateAsync(request, cancellationToken);

        return TypedResults.Created($"/{order.Id}", order);
    }

    private static async Task<Results<Ok<Order>, NotFound>> UpdateOrderStatus(
        Guid id,
        UpdateOrderStatusRequest request,
        IOrdersService ordersService,
        CancellationToken cancellationToken
    )
    {
        var order = await ordersService.UpdateStatusAsync(id, request, cancellationToken);

        return order is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(order);
    }

    private static async Task<Results<NoContent, NotFound>> DeleteOrder(
        Guid id,
        IOrdersService ordersService,
        CancellationToken cancellationToken
    )
    {
        var deleted = await ordersService.DeleteAsync(id, cancellationToken);

        return deleted
            ? TypedResults.NoContent()
            : TypedResults.NotFound();
    }
}