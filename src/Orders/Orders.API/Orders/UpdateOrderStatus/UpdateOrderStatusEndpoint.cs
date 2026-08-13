namespace Orders.API.Orders.UpdateOrderStatus;

public record UpdateOrderStatusRequest(string Status);

public class UpdateOrderStatusEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/api/orders/{id}/status", async (
            string id,
            UpdateOrderStatusRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new UpdateOrderStatusCommand(id, request.Status), cancellationToken);
            return Results.Ok(result);
        })
            .WithName("UpdateOrderStatus")
            .Produces<UpdateOrderStatusResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Cambia el estado de una orden")
            .WithDescription(
                "Valida la transición de estado antes de aplicarla. Transiciones válidas: " +
                "Pending -> Confirmed, Pending -> Cancelled. Cualquier otra combinación responde 400.");
    }
}
