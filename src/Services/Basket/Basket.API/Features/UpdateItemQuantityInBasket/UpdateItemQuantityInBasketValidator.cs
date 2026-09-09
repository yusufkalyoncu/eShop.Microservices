using FluentValidation;

namespace Basket.API.Features.UpdateItemQuantityInBasket;

public class UpdateItemQuantityInBasketValidator : AbstractValidator<UpdateItemQuantityInBasketCommand>
{
    public UpdateItemQuantityInBasketValidator()
    {
        RuleFor(x => x.UserName).NotEmpty().WithMessage("UserName is required");
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("ProductId is required");
        RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0).WithMessage("Quantity must be 0 or greater");
        RuleFor(x => x.Quantity).LessThanOrEqualTo(10).WithMessage("Maximum allowed quantity per item is 10");
    }
}