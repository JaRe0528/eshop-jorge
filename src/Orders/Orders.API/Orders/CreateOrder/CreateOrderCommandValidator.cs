using FluentValidation;

namespace Orders.API.Orders.CreateOrder;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty().WithMessage("El customerId es requerido.");
        RuleFor(x => x.BasketId).NotEmpty().WithMessage("El basketId es requerido.");
    }
}
