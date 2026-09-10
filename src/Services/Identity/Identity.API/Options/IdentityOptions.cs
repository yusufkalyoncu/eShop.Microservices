using BuildingBlocks.Core.Options;
using FluentValidation;

namespace Identity.API.Options;

public class IdentityOptions : IAppOption
{
    public static string SectionName => "Identity";

    public string Authority { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
}

public class IdentityOptionsValidator : AbstractValidator<IdentityOptions>
{
    public IdentityOptionsValidator()
    {
        RuleFor(x => x.Authority).NotEmpty();
        RuleFor(x => x.Audience).NotEmpty();
    }
}