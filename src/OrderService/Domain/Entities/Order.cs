namespace OrderService.Domain.Entities;

public sealed record Order
{
    public Guid Id { get; init; }

    public string UserName { get; init; } = string.Empty;

    public string ProductName { get; init; } = string.Empty;

    public int Quantity { get; init; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; init; }
}