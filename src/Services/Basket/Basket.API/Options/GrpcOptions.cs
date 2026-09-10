using BuildingBlocks.Core.Options;

namespace Basket.API.Options;

public class GrpcOptions : IAppOption
{
    public static string SectionName => "GrpcSettings";

    public string CatalogUrl { get; init; } = null!;
}