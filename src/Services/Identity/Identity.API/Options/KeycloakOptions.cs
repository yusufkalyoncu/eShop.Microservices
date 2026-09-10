using BuildingBlocks.Core.Options;
using FluentValidation;

namespace Identity.API.Options;

public class KeycloakOptions : IAppOption
{
    public static string SectionName => "Keycloak";

    public string Authority { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string Realm { get; init; } = string.Empty;
    public string ClientId { get; init; } = string.Empty;
    public string ClientSecret { get; init; } = string.Empty;
}

public class KeycloakOptionsValidator : AbstractValidator<KeycloakOptions>
{
    public KeycloakOptionsValidator()
    {
        RuleFor(x => x.Authority).NotEmpty();
        RuleFor(x => x.Url).NotEmpty();
        RuleFor(x => x.Realm).NotEmpty().WithMessage("Keycloak Realm is required.");
        RuleFor(x => x.ClientId).NotEmpty().WithMessage("Keycloak ClientId is required.");
        RuleFor(x => x.ClientSecret).NotEmpty().WithMessage("Keycloak ClientSecret is required.");
    }
}