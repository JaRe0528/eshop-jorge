namespace Orders.API.Orders.UpdateOrderStatus;

public record UpdateOrderStatusCommand(string Id, string Status) : ICommand<UpdateOrderStatusResult>;

public record UpdateOrderStatusResult(string Id, string Status);
