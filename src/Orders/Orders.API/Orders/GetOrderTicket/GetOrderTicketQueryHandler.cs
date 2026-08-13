using System.Globalization;
using System.Text;
using Orders.API.Data;
using Orders.API.Exceptions;

namespace Orders.API.Orders.GetOrderTicket;

public class GetOrderTicketQueryHandler(IOrdersRepository repository)
    : IqueryHandler<GetOrderTicketQuery, OrderTicketDto>
{
    // Ancho típico de una impresora térmica de tickets (32 caracteres).
    private const int Width = 32;
    private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("es-MX");

    public async Task<OrderTicketDto> Handle(GetOrderTicketQuery query, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(query.Id, cancellationToken)
            ?? throw new OrderNotFoundException(query.Id);

        var ticket = BuildTicketText(order);
        return new OrderTicketDto(order.Id, ticket);
    }

    // Todo el "diseño" del ticket vive aquí, en el backend: el frontend solo lo muestra tal cual.
    public static string BuildTicketText(Order order)
    {
        var sb = new StringBuilder();

        sb.AppendLine(new string('=', Width));
        sb.AppendLine(Center("E-SHOP"));
        sb.AppendLine(Center("Ticket de compra"));
        sb.AppendLine(new string('=', Width));
        sb.AppendLine($"Orden:   {order.Id}");
        sb.AppendLine($"Cliente: {order.CustomerId}");
        sb.AppendLine($"Fecha:   {order.CreatedAt.ToLocalTime():yyyy-MM-dd HH:mm}");
        sb.AppendLine($"Estado:  {order.Status}");
        sb.AppendLine(new string('-', Width));

        foreach (var item in order.Items)
        {
            sb.AppendLine(Truncate(item.ProductName, Width));
            var qtyPrice = $"{item.Quantity} x {item.UnitPrice.ToString("C", Culture)}";
            var lineTotal = item.LineTotal.ToString("C", Culture);
            sb.AppendLine(JustifyLeftRight(qtyPrice, lineTotal));
        }

        sb.AppendLine(new string('-', Width));
        sb.AppendLine(JustifyLeftRight("Subtotal:", order.Subtotal.ToString("C", Culture)));
        sb.AppendLine(JustifyLeftRight("Impuestos:", order.Tax.ToString("C", Culture)));
        sb.AppendLine(new string('-', Width));
        sb.AppendLine(JustifyLeftRight("TOTAL:", order.Total.ToString("C", Culture)));
        sb.AppendLine(new string('=', Width));
        sb.AppendLine(Center("¡Gracias por tu compra!"));
        sb.AppendLine(new string('=', Width));

        return sb.ToString();
    }

    private static string Center(string text)
    {
        if (text.Length >= Width) return text;
        var padding = (Width - text.Length) / 2;
        return new string(' ', padding) + text;
    }

    private static string Truncate(string text, int max) =>
        text.Length <= max ? text : text[..max];

    private static string JustifyLeftRight(string left, string right)
    {
        var spaces = Width - left.Length - right.Length;
        if (spaces < 1) spaces = 1;
        return left + new string(' ', spaces) + right;
    }
}
