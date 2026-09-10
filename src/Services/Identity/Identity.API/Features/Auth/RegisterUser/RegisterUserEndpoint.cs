using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Web.Endpoints;
using BuildingBlocks.Web.Extensions;
using Identity.API.Features.Auth.LoginUser;

namespace Identity.API.Features.Auth.RegisterUser;

public sealed class RegisterUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/register", async (RegisterUserCommand command, ICommandHandler<RegisterUserCommand, LoginResponse> handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(command, ct);
            return result.Match();
        })
        .WithTags("Auth")
        .WithSummary("Registers a new user in Keycloak and logs them in")
        .WithDescription("Creates a new user account in Keycloak and returns a JWT")
        .Produces<LoginResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}