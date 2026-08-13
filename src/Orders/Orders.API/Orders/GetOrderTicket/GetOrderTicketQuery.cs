namespace Orders.API.Orders.GetOrderTicket;

public record GetOrderTicketQuery(string Id) : IQuery<OrderTicketDto>;

public record OrderTicketDto(string Id, string Text);
