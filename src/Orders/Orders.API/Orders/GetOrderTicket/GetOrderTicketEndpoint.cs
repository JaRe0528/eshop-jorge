namespace Orders.API.Orders.GetOrderTicket;

public class GetOrderTicketEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/orders/{id}/ticket", async (string id, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetOrderTicketQuery(id), cancellationToken);
            return Results.Ok(result);
        })
            .WithName("GetOrderTicket")
            .Produces<OrderTicketDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Genera el ticket virtual de una orden")
            .WithDescription(
                "Arma el texto completo del ticket de compra (encabezado, items, subtotal, " +
                "impuestos y total) del lado del servidor. El frontend solo lo muestra e imprime tal cual.");
    }
}
