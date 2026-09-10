using BuildingBlocks.Core.Results;

namespace Identity.API.Domain.Errors;

public static class IdentityErrors
{
    public static class Auth
    {
        public static readonly Error RegisterFailed = Error.BadRequest(
            "Identity.Auth.RegisterFailed",
            "Failed to create user in Keycloak. User might already exist.");

        public static Error LoginFailed(string details) => Error.BadRequest(
            "Identity.Auth.LoginFailed",
            $"Login failed: {details}");

        public static readonly Error InvalidTokenResponse = Error.InternalServerError(
            "Identity.Auth.InvalidTokenResponse",
            "Failed to deserialize token response from Keycloak.");
    }
}