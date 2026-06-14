namespace OrderService.Application.DTOs;

public sealed record CreateOrderRequest(
    string UserName,
    string ProductName,
    int Quantity
);