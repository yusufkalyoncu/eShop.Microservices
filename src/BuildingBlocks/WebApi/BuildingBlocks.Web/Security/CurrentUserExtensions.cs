using BuildingBlocks.Core.Exceptions;
using BuildingBlocks.Core.Results;

namespace BuildingBlocks.Web.Security;

public static class CurrentUserExtensions
{
    extension(ICurrentUser currentUser)
    {
        public string GetRequiredName()
        {
            return currentUser.Name ?? throw new AppException(Error.Unauthorized("User.NotAuthenticated", "User name claim is missing or user is not authenticated."));
        }

        public string GetRequiredId()
        {
            return currentUser.Id ?? throw new AppException(Error.Unauthorized("User.NotAuthenticated", "User ID claim is missing or user is not authenticated."));
        }
    }
}