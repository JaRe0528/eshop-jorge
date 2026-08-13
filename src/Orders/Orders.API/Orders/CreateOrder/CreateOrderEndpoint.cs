namespace Orders.API.Orders.CreateOrder;

public record CreateOrderRequest(string CustomerId, string BasketId);

public class CreateOrderEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/orders", async (
            CreateOrderRequest request,
            HttpRequest httpRequest,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            string? idempotencyKey = httpRequest.Headers["Idempotency-Key"].FirstOrDefault();

            var command = new CreateOrderCommand(request.CustomerId, request.BasketId, idempotencyKey);
            var result = await sender.Send(command, cancellationToken);

            return Results.Created($"/api/orders/{result.Id}", result);
        })
            .WithName("CreateOrder")
            .Produces<CreateOrderResult>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Genera una orden de compra a partir del carrito del cliente")
            .WithDescription(
                "Valida el carrito contra Basket.API, calcula subtotal/impuestos/total, " +
                "persiste la orden en MongoDB Atlas y soporta idempotencia mediante el " +
                "header Idempotency-Key (si se reenvía la misma clave, regresa la orden ya creada).");
    }
}
