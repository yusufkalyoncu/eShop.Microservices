using BuildingBlocks.Core.Results;
using Identity.API.Features.Auth.LoginUser;

namespace Identity.API.Domain.Services;

public interface IIdentityService
{
    Task<Result<LoginResponse>> LoginAsync(string username, string password, CancellationToken cancellationToken = default);
    Task<Result<string>> RegisterUserAsync(string username, string email, string firstName, string lastName, string password, CancellationToken cancellationToken = default);
}