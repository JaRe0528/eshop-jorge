namespace Orders.API.Services;

public interface IBasketApiClient
{
    // Regresa null si el carrito no existe (Basket.API responde 404 en ese caso).
    Task<BasketDto?> GetBasketAsync(string userName, CancellationToken cancellationToken);
}
