using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Orders.API.Models;

// Este es el documento tal cual se guarda en la colección "orders" de MongoDB Atlas.
public class Order
{
    [BsonId]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string CustomerId { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonRepresentation(BsonType.String)]
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public List<OrderItem> Items { get; set; } = new();

    public decimal Subtotal { get; set; }

    public decimal Tax { get; set; }

    public decimal Total { get; set; }

    // Soporte de idempotencia: si el cliente reenvía la misma Idempotency-Key,
    // devolvemos esta misma orden en vez de crear una nueva (ver OrdersRepository,
    // que crea un índice único parcial sobre este campo).
    public string? IdempotencyKey { get; set; }
}
