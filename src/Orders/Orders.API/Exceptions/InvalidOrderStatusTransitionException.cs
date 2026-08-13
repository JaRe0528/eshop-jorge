using BuildingBlocks.Exceptions;

namespace Orders.API.Exceptions;

public class InvalidOrderStatusTransitionException : BadRequestException
{
    public InvalidOrderStatusTransitionException(string from, string to)
        : base($"No se puede cambiar el estado de la orden de \"{from}\" a \"{to}\".")
    {
    }
}
