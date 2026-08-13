namespace Orders.API.Services;

// Estos DTOs reflejan EXACTAMENTE el JSON que devuelve Basket.API (ShoppingCart.cs),
// incluyendo el nombre real de sus campos.
public class BasketDto
{
    public string UserName { get; set; } = default!;
    public List<BasketItemDto> Items { get; set; } = new();

    // Ojo: Basket.API tiene un typo real en su modelo ("TatalPrice", no "TotalPrice").
    // No lo usamos para calcular el total de la orden (lo recalculamos nosotros
    // desde los items), pero lo dejamos mapeado por si se necesita para depurar.
    public decimal TatalPrice { get; set; }
}

public class BasketItemDto
{
    public int Quantity { get; set; }
    public string Color { get; set; } = default!;
    public decimal Price { get; set; }
    public string ProductId { get; set; } = default!;
    public string ProductName { get; set; } = default!;
    public string ImageFile { get; set; } = default!;
    public string ImageUrl { get; set; } = default!;
}
