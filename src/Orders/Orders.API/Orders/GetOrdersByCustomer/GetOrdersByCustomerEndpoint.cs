using Orders.API.Orders.GetOrderById;

namespace Orders.API.Orders.GetOrdersByCustomer;

public class GetOrdersByCustomerEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/orders/customer/{customerId}", async (
            string customerId, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetOrdersByCustomerQuery(customerId), cancellationToken);
            return Results.Ok(result);
        })
            .WithName("GetOrdersByCustomer")
            .Produces<List<OrderDto>>(StatusCodes.Status200OK)
            .WithSummary("Lista las órdenes de un cliente")
            .WithDescription("Devuelve un arreglo (vacío si no tiene órdenes) con el historial de compras del cliente, más recientes primero.");
    }
}
