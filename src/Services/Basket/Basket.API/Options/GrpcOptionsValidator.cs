using FluentValidation;

namespace Basket.API.Options;

public class GrpcOptionsValidator : AbstractValidator<GrpcOptions>
{
    public GrpcOptionsValidator()
    {
        RuleFor(x => x.CatalogUrl)
            .NotEmpty().WithMessage("CatalogUrl is required in GrpcSettings")
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _)).WithMessage("CatalogUrl must be a valid absolute URI");
    }
}