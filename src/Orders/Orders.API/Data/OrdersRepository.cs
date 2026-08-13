using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Orders.API.Data;

// Registrado como Singleton en Program.cs: MongoClient/IMongoCollection son
// thread-safe y están pensados para reutilizarse durante toda la vida de la app.
public class OrdersRepository : IOrdersRepository
{
    private readonly IMongoCollection<Order> _orders;

    public OrdersRepository(IConfiguration configuration, IOptions<MongoOrdersSettings> settings)
    {
        var connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException(
                "Falta configurar la cadena de conexión 'Database' de MongoDB Atlas.");

        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _orders = database.GetCollection<Order>(settings.Value.CollectionName);

        // Índice único PARCIAL sobre IdempotencyKey: solo aplica a documentos que
        // sí tengan ese campo, así que varias órdenes sin Idempotency-Key conviven
        // sin problema, pero nunca puede haber dos órdenes con la MISMA clave.
        var indexKeys = Builders<Order>.IndexKeys.Ascending(o => o.IdempotencyKey);
        var indexOptions = new CreateIndexOptions<Order>
        {
            Unique = true,
            PartialFilterExpression = Builders<Order>.Filter.Exists(o => o.IdempotencyKey)
        };
        _orders.Indexes.CreateOne(new CreateIndexModel<Order>(indexKeys, indexOptions));
    }

    public async Task<Order> CreateAsync(Order order, CancellationToken cancellationToken)
    {
        await _orders.InsertOneAsync(order, cancellationToken: cancellationToken);
        return order;
    }

    public async Task<Order?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        return await _orders.Find(o => o.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Order?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken)
    {
        return await _orders.Find(o => o.IdempotencyKey == idempotencyKey).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<Order>> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken)
    {
        return await _orders.Find(o => o.CustomerId == customerId)
            .SortByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateStatusAsync(string id, OrderStatus newStatus, CancellationToken cancellationToken)
    {
        var update = Builders<Order>.Update.Set(o => o.Status, newStatus);
        await _orders.UpdateOneAsync(o => o.Id == id, update, cancellationToken: cancellationToken);
    }
}
