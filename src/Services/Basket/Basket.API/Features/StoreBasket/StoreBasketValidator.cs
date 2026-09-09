using FluentValidation;

namespace Basket.API.Features.StoreBasket;

public class StoreBasketCommandValidator : AbstractValidator<StoreBasketCommand>
{
    public StoreBasketCommandValidator()
    {
        RuleFor(x => x.Cart).NotNull().WithMessage("Cart can not be null");
        RuleFor(x => x.Cart.UserName).NotEmpty().WithMessage("UserName is required");
        RuleForEach(x => x.Cart.Items).ChildRules(items => 
        {
            items.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than 0");
            items.RuleFor(i => i.Quantity).LessThanOrEqualTo(10).WithMessage("Maximum allowed quantity per item is 10");
        });
    }
}