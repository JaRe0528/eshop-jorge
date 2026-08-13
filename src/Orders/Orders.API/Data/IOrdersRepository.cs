namespace Orders.API.Data;

public interface IOrdersRepository
{
    Task<Order> CreateAsync(Order order, CancellationToken cancellationToken);
    Task<Order?> GetByIdAsync(string id, CancellationToken cancellationToken);
    Task<Order?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken);
    Task<List<Order>> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken);
    Task UpdateStatusAsync(string id, OrderStatus newStatus, CancellationToken cancellationToken);
}
