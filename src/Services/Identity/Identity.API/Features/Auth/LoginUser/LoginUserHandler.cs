using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Core.Results;
using Identity.API.Domain.Services;

namespace Identity.API.Features.Auth.LoginUser;

internal sealed class LoginUserHandler(
    IIdentityService identityService) 
    : ICommandHandler<LoginUserCommand, LoginResponse>
{
    public async Task<Result<LoginResponse>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        return await identityService.LoginAsync(request.Username, request.Password, cancellationToken);
    }
}