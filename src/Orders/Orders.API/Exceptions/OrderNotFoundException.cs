using BuildingBlocks.Exceptions;

namespace Orders.API.Exceptions;

public class OrderNotFoundException : NotFoundException
{
    public OrderNotFoundException(string id) : base("Order", id)
    {
    }
}
