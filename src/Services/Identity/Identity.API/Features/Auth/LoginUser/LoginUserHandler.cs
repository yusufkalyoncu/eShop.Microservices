using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Core.Results;
using Identity.API.Domain.Errors;
using Microsoft.Extensions.Options;
using Identity.API.Options;

namespace Identity.API.Features.Auth.LoginUser;

internal sealed class LoginUserHandler(
    IHttpClientFactory httpClientFactory, 
    IOptions<IdentityOptions> identityOptions,
    IOptions<KeycloakOptions> keycloakOptions) 
    : ICommandHandler<LoginUserCommand, LoginResponse>
{
    public async Task<Result<LoginResponse>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient("Keycloak");

        var content = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("client_id", identityOptions.Value.Audience),
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("username", request.Username),
            new KeyValuePair<string, string>("password", request.Password)
        ]);

        var response = await client.PostAsync($"/realms/{keycloakOptions.Value.Realm}/protocol/openid-connect/token", content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            return Result.Failure<LoginResponse>(IdentityErrors.Auth.LoginFailed(error));
        }

        var tokenResponse = await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: cancellationToken);
        
        if (tokenResponse == null)
        {
            return Result.Failure<LoginResponse>(IdentityErrors.Auth.InvalidTokenResponse);
        }

        return tokenResponse;
    }
}