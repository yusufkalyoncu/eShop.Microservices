using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Core.Results;
using BuildingBlocks.Outbox.Abstractions;
using Identity.API.Domain.Services;
using Identity.API.Features.Auth.LoginUser;
using Identity.API.Infrastructure.Data;
using Identity.Contracts.IntegrationEvents;

namespace Identity.API.Features.Auth.RegisterUser;

internal sealed class RegisterUserHandler(
    IIdentityService identityService,
    IOutboxService outboxService,
    IdentityDbContext dbContext)
    : ICommandHandler<RegisterUserCommand, LoginResponse>
{
    public async Task<Result<LoginResponse>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var registerResult = await identityService.RegisterUserAsync(
            request.Username, 
            request.Email, 
            request.FirstName, 
            request.LastName, 
            request.Password, 
            cancellationToken);

        if (registerResult.IsFailure)
        {
            return Result.Failure<LoginResponse>(registerResult.Error);
        }

        // Publish UserRegisteredIntegrationEvent via Outbox
        var integrationEvent = new UserRegisteredIntegrationEvent(
            EventId: Guid.NewGuid(),
            UserId: registerResult.Data,
            Email: request.Email,
            FirstName: request.FirstName,
            LastName: request.LastName,
            RegisteredAt: DateTime.UtcNow
        );

        await outboxService.AddAsync(integrationEvent, cancellationToken: cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        // Login user to get the token directly
        return await identityService.LoginAsync(request.Username, request.Password, cancellationToken);
    }
}