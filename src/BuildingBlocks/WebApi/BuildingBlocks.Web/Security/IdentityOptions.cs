using BuildingBlocks.Core.Options;
using FluentValidation;

namespace BuildingBlocks.Web.Security;

public class IdentityOptions : IAppOption
{
    public static string SectionName => "Identity";

    public string Authority { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string[]? ValidIssuers { get; init; }
}

public class IdentityOptionsValidator : AbstractValidator<IdentityOptions>
{
    public IdentityOptionsValidator()
    {
        RuleFor(x => x.Authority).NotEmpty();
        RuleFor(x => x.Audience).NotEmpty();
    }
}