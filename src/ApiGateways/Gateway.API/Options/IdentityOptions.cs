using BuildingBlocks.Core.Options;

namespace Gateway.API.Options;

public class IdentityOptions : IAppOption
{
    public static string SectionName => "Identity";
    
    public string Authority { get; init; } = null!;
    public string Audience { get; init; } = null!;
}