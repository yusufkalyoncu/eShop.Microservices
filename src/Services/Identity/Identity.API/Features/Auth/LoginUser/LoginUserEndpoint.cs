using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Web.Endpoints;
using BuildingBlocks.Web.Extensions;

namespace Identity.API.Features.Auth.LoginUser;

public sealed class LoginUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/login", async (LoginUserCommand command, ICommandHandler<LoginUserCommand, LoginResponse> handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(command, ct);
            return result.Match();
        })
        .WithTags("Auth")
        .WithSummary("Authenticates a user and returns a JWT")
        .WithDescription("Uses Resource Owner Password Credentials flow to authenticate against Keycloak")
        .Produces<LoginResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}