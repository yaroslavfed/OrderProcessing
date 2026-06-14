namespace Orders.Contracts.Events;

public sealed record OrderCreatedEvent(
    Guid OrderId,
    string UserName,
    string ProductName,
    int Quantity,
    DateTime CreatedAt
);