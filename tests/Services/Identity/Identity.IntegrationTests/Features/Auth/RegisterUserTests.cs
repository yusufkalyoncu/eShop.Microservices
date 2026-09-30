using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Identity.API.Features.Auth.RegisterUser;
using Identity.IntegrationTests.Infrastructure;

namespace Identity.IntegrationTests.Features.Auth;

public class RegisterUserTests(IdentityApiFactory factory) : IClassFixture<IdentityApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Should_Register_New_User_Successfully()
    {
        // Arrange
        var command = new RegisterUserCommand(
            "newuser",
            "newuser@example.com",
            "New",
            "User",
            "Test1234!"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/auth/register", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<Identity.API.Features.Auth.LoginUser.LoginResponse>();
        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Should_Return_Conflict_When_User_Already_Exists()
    {
        // Arrange
        var command = new RegisterUserCommand(
            "existinguser",
            "existinguser@example.com",
            "Existing",
            "User",
            "Test1234!"
        );

        // Act 1: First registration should succeed
        var firstResponse = await _client.PostAsJsonAsync("/auth/register", command);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act 2: Second registration with same username/email should fail
        var secondResponse = await _client.PostAsJsonAsync("/auth/register", command);

        // Assert
        secondResponse.StatusCode.Should().Be(HttpStatusCode.Conflict); // Standard mapping for Conflict
    }
}