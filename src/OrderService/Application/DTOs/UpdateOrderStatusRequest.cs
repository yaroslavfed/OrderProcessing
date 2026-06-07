namespace OrderService.Application.DTOs;

public sealed record UpdateOrderStatusRequest(
    string Status
);