using Orders.API.Data;
using Orders.API.Orders.GetOrderById;

namespace Orders.API.Orders.GetOrdersByCustomer;

public class GetOrdersByCustomerQueryHandler(IOrdersRepository repository)
    : IqueryHandler<GetOrdersByCustomerQuery, List<OrderDto>>
{
    public async Task<List<OrderDto>> Handle(GetOrdersByCustomerQuery query, CancellationToken cancellationToken)
    {
        var orders = await repository.GetByCustomerIdAsync(query.CustomerId, cancellationToken);
        return orders.Select(GetOrderByIdQueryHandler.MapToDto).ToList();
    }
}
