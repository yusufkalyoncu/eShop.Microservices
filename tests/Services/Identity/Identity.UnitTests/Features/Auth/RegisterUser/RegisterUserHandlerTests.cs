using BuildingBlocks.Core.Results;
using BuildingBlocks.Outbox.Abstractions;
using FluentAssertions;
using Identity.API.Domain.Services;
using Identity.API.Features.Auth.LoginUser;
using Identity.API.Features.Auth.RegisterUser;
using Identity.API.Infrastructure.Data;
using Identity.Contracts.IntegrationEvents;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Identity.UnitTests.Features.Auth.RegisterUser;

public class RegisterUserHandlerTests
{
    private readonly IIdentityService _identityServiceMock;
    private readonly IOutboxService _outboxServiceMock;
    private readonly RegisterUserHandler _handler;

    public RegisterUserHandlerTests()
    {
        _identityServiceMock = Substitute.For<IIdentityService>();
        _outboxServiceMock = Substitute.For<IOutboxService>();

        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var dbContext = new IdentityDbContext(options);

        _handler = new RegisterUserHandler(_identityServiceMock, _outboxServiceMock, dbContext);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenRegistrationFails()
    {
        // Arrange
        var command = new RegisterUserCommand("testuser", "test@test.com", "John", "Doe", "Password123!");
        var expectedError = Error.BadRequest("Auth.RegistrationFailed", "User already exists.");

        _identityServiceMock.RegisterUserAsync(command.Username, command.Email, command.FirstName, command.LastName, command.Password, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<string>(expectedError));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(expectedError);

        await _outboxServiceMock.DidNotReceiveWithAnyArgs().AddAsync(
            Arg.Any<UserRegisteredIntegrationEvent>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPublishEventAndLogin_WhenRegistrationIsSuccessful()
    {
        // Arrange
        var command = new RegisterUserCommand("testuser", "test@test.com", "John", "Doe", "Password123!");
        var userId = Guid.NewGuid().ToString();
        var expectedLoginResponse = new LoginResponse { AccessToken = "test-token-string" };

        _identityServiceMock.RegisterUserAsync(command.Username, command.Email, command.FirstName, command.LastName, command.Password, Arg.Any<CancellationToken>())
            .Returns(Result.Success(userId));

        _identityServiceMock.LoginAsync(command.Username, command.Password, Arg.Any<CancellationToken>())
            .Returns(Result.Success(expectedLoginResponse));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.AccessToken.Should().Be(expectedLoginResponse.AccessToken);

        await _outboxServiceMock.Received(1).AddAsync(
            Arg.Is<UserRegisteredIntegrationEvent>(e => 
                e.UserId == userId &&
                e.Email == command.Email &&
                e.FirstName == command.FirstName &&
                e.LastName == command.LastName),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }
}