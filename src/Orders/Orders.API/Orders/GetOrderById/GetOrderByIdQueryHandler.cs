using Orders.API.Data;
using Orders.API.Exceptions;

namespace Orders.API.Orders.GetOrderById;

public class GetOrderByIdQueryHandler(IOrdersRepository repository) : IqueryHandler<GetOrderByIdQuery, OrderDto>
{
    public async Task<OrderDto> Handle(GetOrderByIdQuery query, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(query.Id, cancellationToken)
            ?? throw new OrderNotFoundException(query.Id);

        return MapToDto(order);
    }

    // Se reutiliza también desde GetOrdersByCustomer para no duplicar el mapeo.
    public static OrderDto MapToDto(Order order) => new(
        order.Id,
        order.CustomerId,
        order.CreatedAt,
        order.Status.ToString(),
        order.Items
            .Select(i => new OrderItemDto(i.ProductId, i.ProductName, i.Quantity, i.UnitPrice, i.LineTotal))
            .ToList(),
        order.Subtotal,
        order.Tax,
        order.Total);
}
