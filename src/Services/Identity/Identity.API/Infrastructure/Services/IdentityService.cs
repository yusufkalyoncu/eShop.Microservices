using System.Net;
using BuildingBlocks.Core.Results;
using BuildingBlocks.Web.Security;
using Flurl.Http;
using Identity.API.Domain.Errors;
using Identity.API.Domain.Services;
using Identity.API.Features.Auth.LoginUser;
using Keycloak.Net;
using Keycloak.Net.Models.Users;
using Microsoft.Extensions.Options;
using KeycloakOptions = Identity.API.Options.KeycloakOptions;

namespace Identity.API.Infrastructure.Services;

internal sealed class IdentityService(
    KeycloakClient keycloakClient,
    IHttpClientFactory httpClientFactory,
    IOptions<IdentityOptions> identityOptions,
    IOptions<KeycloakOptions> keycloakOptions) : IIdentityService
{
    public async Task<Result<LoginResponse>> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("Keycloak");

        var content = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("client_id", identityOptions.Value.Audience),
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("username", username),
            new KeyValuePair<string, string>("password", password)
        ]);

        var response = await client.PostAsync($"/realms/{keycloakOptions.Value.Realm}/protocol/openid-connect/token", content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(error);
                if (doc.RootElement.TryGetProperty("error", out var errorProp) && errorProp.GetString() == "invalid_grant")
                {
                    return Result.Failure<LoginResponse>(IdentityErrors.Auth.InvalidCredentials);
                }
            }
            catch
            {
                // Ignored, fallback to unknown error
            }
            return Result.Failure<LoginResponse>(IdentityErrors.Auth.UnknownError(error));
        }

        var tokenResponse = await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: cancellationToken);
        
        if (tokenResponse == null)
        {
            return Result.Failure<LoginResponse>(IdentityErrors.Auth.InvalidTokenResponse);
        }

        return tokenResponse;
    }

    public async Task<Result<string>> RegisterUserAsync(string username, string email, string firstName, string lastName, string password, CancellationToken cancellationToken = default)
    {
        var user = new User
        {
            UserName = username,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Enabled = true,
            EmailVerified = true,
            RequiredActions = [],
            Credentials =
            [
                new Credentials
                {
                    Type = "password",
                    Value = password,
                    Temporary = false
                }
            ]
        };

        try
        {
            var success = await keycloakClient.CreateUserAsync(keycloakOptions.Value.Realm, user, cancellationToken);
            if (!success)
            {
                return Result.Failure<string>(IdentityErrors.Auth.RegisterFailed);
            }
        }
        catch (FlurlHttpException ex) when (ex.StatusCode == (int)HttpStatusCode.Conflict)
        {
            return Result.Failure<string>(IdentityErrors.Auth.UserAlreadyExists);
        }
        catch (Exception)
        {
            return Result.Failure<string>(IdentityErrors.Auth.RegisterFailed);
        }

        // Fetch the user to get their ID for role mapping and integration event
        var users = await keycloakClient.GetUsersAsync(keycloakOptions.Value.Realm, username: username, cancellationToken: cancellationToken);
        var createdUser = users.FirstOrDefault();

        if (createdUser == null)
        {
            return Result.Failure<string>(IdentityErrors.Auth.RegisterFailed);
        }

        // Assign 'user' role
        var realmRoles = await keycloakClient.GetRolesAsync(keycloakOptions.Value.Realm, cancellationToken: cancellationToken);
        var userRole = realmRoles.FirstOrDefault(r => r.Name == BuildingBlocks.Web.Security.Roles.User);

        if (userRole != null)
        {
            await keycloakClient.AddRealmRoleMappingsToUserAsync(keycloakOptions.Value.Realm, createdUser.Id, new[] { userRole }, cancellationToken);
        }

        return Result.Success(createdUser.Id);
    }
}