using FluentAssertions;
using Identity.API.Domain.Services;
using Identity.API.Features.Auth.LoginUser;
using BuildingBlocks.Core.Results;
using NSubstitute;

namespace Identity.UnitTests.Features.Auth.LoginUser;

public class LoginUserHandlerTests
{
    private readonly IIdentityService _identityServiceMock;
    private readonly LoginUserHandler _handler;

    public LoginUserHandlerTests()
    {
        _identityServiceMock = Substitute.For<IIdentityService>();
        _handler = new LoginUserHandler(_identityServiceMock);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenCredentialsAreValid()
    {
        // Arrange
        var command = new LoginUserCommand("testuser", "Password123!");
        var expectedResponse = new LoginResponse { AccessToken = "test-token-string" };

        _identityServiceMock.LoginAsync(command.Username, command.Password, Arg.Any<CancellationToken>())
            .Returns(Result.Success(expectedResponse));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.AccessToken.Should().Be(expectedResponse.AccessToken);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCredentialsAreInvalid()
    {
        // Arrange
        var command = new LoginUserCommand("testuser", "WrongPassword!");
        var expectedError = Error.Unauthorized("Auth.InvalidCredentials", "Invalid credentials.");

        _identityServiceMock.LoginAsync(command.Username, command.Password, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<LoginResponse>(expectedError));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(expectedError);
    }
}