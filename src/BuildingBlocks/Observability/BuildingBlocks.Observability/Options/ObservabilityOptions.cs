using BuildingBlocks.Core.Options;
using FluentValidation;

namespace BuildingBlocks.Observability.Options;

public sealed class ObservabilityOptions : IAppOption
{
    public const string SectionName = "Observability";
    static string IAppOption.SectionName => SectionName;

    public string? ServiceName { get; init; }
    public string ServiceVersion { get; init; } = "1.0.0";
    public string OtlpEndpoint { get; init; } = "http://localhost:4317";
}

public class ObservabilityOptionsValidator : AbstractValidator<ObservabilityOptions>
{
    public ObservabilityOptionsValidator()
    {
        RuleFor(x => x.OtlpEndpoint).NotEmpty();
        RuleFor(x => x.ServiceVersion).NotEmpty();
    }
}