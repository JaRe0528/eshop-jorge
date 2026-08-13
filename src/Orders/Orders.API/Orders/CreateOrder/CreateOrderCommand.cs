namespace Orders.API.Orders.CreateOrder;

public record CreateOrderCommand(string CustomerId, string BasketId, string? IdempotencyKey)
    : ICommand<CreateOrderResult>;

public record CreateOrderResult(
    string Id,
    string CustomerId,
    string Status,
    decimal Subtotal,
    decimal Tax,
    decimal Total,
    DateTime CreatedAt);
