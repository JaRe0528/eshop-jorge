using Orders.API.Orders.GetOrderById;

namespace Orders.API.Orders.GetOrdersByCustomer;

public record GetOrdersByCustomerQuery(string CustomerId) : IQuery<List<OrderDto>>;
