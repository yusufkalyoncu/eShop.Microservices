using BuildingBlocks.Core.Results;

namespace Identity.API.Domain.Errors;

public static class IdentityErrors
{
    public static class Auth
    {
        public static readonly Error RegisterFailed = Error.BadRequest(
            "Identity.Auth.RegisterFailed",
            "Failed to create user in Keycloak. User might already exist.");

        public static readonly Error UserAlreadyExists = Error.Conflict(
            "Identity.Auth.UserAlreadyExists",
            "A user with this username or email already exists.");

        public static readonly Error InvalidCredentials = Error.BadRequest(
            "Identity.Auth.InvalidCredentials",
            "Invalid username or password.");

        public static Error UnknownError(string details) => Error.BadRequest(
            "Identity.Auth.UnknownError",
            $"An unknown error occurred: {details}");

        public static readonly Error InvalidTokenResponse = Error.InternalServerError(
            "Identity.Auth.InvalidTokenResponse",
            "Failed to deserialize token response from Keycloak.");
    }
}