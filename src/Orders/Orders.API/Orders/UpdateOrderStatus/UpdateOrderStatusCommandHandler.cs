using BuildingBlocks.Exceptions;
using Orders.API.Data;
using Orders.API.Exceptions;

namespace Orders.API.Orders.UpdateOrderStatus;

public class UpdateOrderStatusCommandHandler(IOrdersRepository repository)
    : ICommandHandler<UpdateOrderStatusCommand, UpdateOrderStatusResult>
{
    // Únicas transiciones de negocio permitidas (punto 4.2 del examen):
    // Pending -> Confirmed, Pending -> Cancelled. Cancelled nunca regresa a Confirmed,
    // y de Confirmed tampoco se permite ningún otro cambio en este alcance.
    private static readonly Dictionary<OrderStatus, OrderStatus[]> AllowedTransitions = new()
    {
        [OrderStatus.Pending] = [OrderStatus.Confirmed, OrderStatus.Cancelled],
        [OrderStatus.Confirmed] = [],
        [OrderStatus.Cancelled] = []
    };

    public async Task<UpdateOrderStatusResult> Handle(UpdateOrderStatusCommand command, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new OrderNotFoundException(command.Id);

        if (!Enum.TryParse<OrderStatus>(command.Status, ignoreCase: true, out var newStatus))
            throw new BadRequestException(
                $"El estado \"{command.Status}\" no es válido. Usa Pending, Confirmed o Cancelled.");

        if (!AllowedTransitions[order.Status].Contains(newStatus))
            throw new InvalidOrderStatusTransitionException(order.Status.ToString(), newStatus.ToString());

        await repository.UpdateStatusAsync(order.Id, newStatus, cancellationToken);

        return new UpdateOrderStatusResult(order.Id, newStatus.ToString());
    }
}
