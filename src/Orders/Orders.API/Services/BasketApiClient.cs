using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BuildingBlocks.Exceptions;

namespace Orders.API.Services;

public class BasketApiClient(HttpClient httpClient) : IBasketApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<BasketDto?> GetBasketAsync(string userName, CancellationToken cancellationToken)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.GetAsync($"/basket/{Uri.EscapeDataString(userName)}", cancellationToken);
        }
        catch (HttpRequestException)
        {
            // Basket.API no responde (caído, mal configurado, red, etc.) -> error controlado, sin stack trace al cliente.
            throw new InternalServerException("No fue posible comunicarse con el servicio de carrito en este momento.");
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        if (!response.IsSuccessStatusCode)
            throw new InternalServerException("No fue posible consultar el carrito del cliente.");

        return await response.Content.ReadFromJsonAsync<BasketDto>(JsonOptions, cancellationToken);
    }
}
