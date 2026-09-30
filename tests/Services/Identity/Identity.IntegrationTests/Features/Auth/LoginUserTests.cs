using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Identity.API.Features.Auth.LoginUser;
using Identity.API.Features.Auth.RegisterUser;
using Identity.IntegrationTests.Infrastructure;

namespace Identity.IntegrationTests.Features.Auth;

public class LoginUserTests(IdentityApiFactory factory) : IClassFixture<IdentityApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Should_Login_And_Return_Token_Successfully()
    {
        // Arrange
        var registerCommand = new RegisterUserCommand(
            "loginuser",
            "loginuser@example.com",
            "Login",
            "User",
            "Test1234!"
        );
        
        // Ensure user exists
        var registerResponse = await _client.PostAsJsonAsync("/auth/register", registerCommand);
        registerResponse.EnsureSuccessStatusCode();

        var loginCommand = new LoginUserCommand("loginuser", "Test1234!");

        // Act
        var response = await _client.PostAsJsonAsync("/auth/login", loginCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Should_Return_Unauthorized_For_Invalid_Credentials()
    {
        // Arrange
        var loginCommand = new LoginUserCommand("nonexistentuser", "WrongPassword!");

        // Act
        var response = await _client.PostAsJsonAsync("/auth/login", loginCommand);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest);
    }
}