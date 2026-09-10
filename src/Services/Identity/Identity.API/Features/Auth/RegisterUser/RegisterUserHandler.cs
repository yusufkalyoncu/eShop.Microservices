using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Core.Results;
using Identity.API.Domain.Errors;
using Identity.API.Features.Auth.LoginUser;
using Keycloak.Net;
using Keycloak.Net.Models.Users;
using Microsoft.Extensions.Options;

namespace Identity.API.Features.Auth.RegisterUser;

internal sealed class RegisterUserHandler(
    KeycloakClient keycloakClient, 
    ICommandHandler<LoginUserCommand, LoginResponse> loginHandler,
    IOptions<Identity.API.Options.KeycloakOptions> keycloakOptions) 
    : ICommandHandler<RegisterUserCommand, LoginResponse>
{
    public async Task<Result<LoginResponse>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var user = new User
        {
            UserName = request.Username,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Enabled = true,
            EmailVerified = true,
            RequiredActions = Array.Empty<string>()
        };

        var success = await keycloakClient.CreateUserAsync(keycloakOptions.Value.Realm, user, cancellationToken);
        
        if (!success)
        {
            return Result.Failure<LoginResponse>(IdentityErrors.Auth.RegisterFailed);
        }

        // Fetch the user to get their ID
        var users = await keycloakClient.GetUsersAsync(keycloakOptions.Value.Realm, username: request.Username, cancellationToken: cancellationToken);
        var createdUser = users.FirstOrDefault();
        
        if (createdUser != null)
        {
            // Set the password explicitly as permanent
            await keycloakClient.ResetUserPasswordAsync(keycloakOptions.Value.Realm, createdUser.Id, request.Password, false, cancellationToken);
        }

        // Login user to get the token
        var loginResult = await loginHandler.Handle(new LoginUserCommand(request.Username, request.Password), cancellationToken);

        return loginResult;
    }
}