using BuildingBlocks.Exceptions;
using MongoDB.Driver;
using Orders.API.Data;
using Orders.API.Exceptions;
using Orders.API.Services;

namespace Orders.API.Orders.CreateOrder;

public class CreateOrderCommandHandler(
    IOrdersRepository repository,
    IBasketApiClient basketApiClient,
    IConfiguration configuration)
    : ICommandHandler<CreateOrderCommand, CreateOrderResult>
{
    public async Task<CreateOrderResult> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        // 1) Idempotencia: si ya existe una orden con esta misma Idempotency-Key,
        //    la regresamos tal cual, sin crear una segunda orden.
        if (!string.IsNullOrWhiteSpace(command.IdempotencyKey))
        {
            var existing = await repository.GetByIdempotencyKeyAsync(command.IdempotencyKey, cancellationToken);
            if (existing is not null)
                return ToResult(existing);
        }

        // 2) Obtenemos y validamos el carrito real desde Basket.API.
        var basket = await basketApiClient.GetBasketAsync(command.BasketId, cancellationToken);
        if (basket is null || basket.Items.Count == 0)
            throw new EmptyBasketException(command.BasketId);

        foreach (var item in basket.Items)
        {
            if (item.Quantity <= 0)
                throw new BadRequestException($"La cantidad del producto \"{item.ProductName}\" no es válida.");
            if (item.Price < 0)
                throw new BadRequestException($"El precio del producto \"{item.ProductName}\" no es válido.");
        }

        // 3) Armamos la orden conservando el precio del carrito al momento de comprar.
        var items = basket.Items.Select(i => new OrderItem
        {
            ProductId = i.ProductId,
            ProductName = i.ProductName,
            Quantity = i.Quantity,
            UnitPrice = i.Price,
            LineTotal = i.Price * i.Quantity
        }).ToList();

        var subtotal = items.Sum(i => i.LineTotal);
        var taxRate = configuration.GetValue<decimal?>("Orders:TaxRate") ?? 0.16m;
        var tax = Math.Round(subtotal * taxRate, 2);
        var total = subtotal + tax;

        var order = new Order
        {
            Id = Guid.NewGuid().ToString(),
            CustomerId = command.CustomerId,
            CreatedAt = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            Items = items,
            Subtotal = subtotal,
            Tax = tax,
            Total = total,
            IdempotencyKey = string.IsNullOrWhiteSpace(command.IdempotencyKey) ? null : command.IdempotencyKey
        };

        try
        {
            await repository.CreateAsync(order, cancellationToken);
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            // Dos solicitudes concurrentes con la misma Idempotency-Key: alguien más
            // ya insertó la orden justo antes que nosotros. Regresamos esa, no fallamos.
            var existing = await repository.GetByIdempotencyKeyAsync(command.IdempotencyKey!, cancellationToken);
            if (existing is not null)
                return ToResult(existing);

            throw new InternalServerException("No se pudo registrar la orden.");
        }
        catch (Exception)
        {
            // Nunca exponemos detalles internos (cadenas de conexión, stack traces, etc.)
            throw new InternalServerException("No se pudo registrar la orden en este momento.");
        }

        return ToResult(order);
    }

    private static CreateOrderResult ToResult(Order order) => new(
        order.Id, order.CustomerId, order.Status.ToString(), order.Subtotal, order.Tax, order.Total, order.CreatedAt);
}
