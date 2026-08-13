using BuildingBlocks.Exceptions;

namespace Orders.API.Exceptions;

public class EmptyBasketException : BadRequestException
{
    public EmptyBasketException(string basketId)
        : base($"El carrito \"{basketId}\" está vacío o no existe. No se puede generar una orden.")
    {
    }
}
