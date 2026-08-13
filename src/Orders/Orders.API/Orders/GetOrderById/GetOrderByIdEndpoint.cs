namespace Orders.API.Orders.GetOrderById;

public class GetOrderByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/orders/{id}", async (string id, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetOrderByIdQuery(id), cancellationToken);
            return Results.Ok(result);
        })
            .WithName("GetOrderById")
            .Produces<OrderDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Consulta una orden por su Id")
            .WithDescription("Devuelve el detalle completo de una orden, incluyendo sus items, subtotal, impuestos y total.");
    }
}
