using Microsoft.AspNetCore.Routing;

namespace BuildingBlocks.Web.Endpoints;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}