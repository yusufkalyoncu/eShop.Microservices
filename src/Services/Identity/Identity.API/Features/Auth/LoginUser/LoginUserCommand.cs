using BuildingBlocks.Core.CQRS;
using System.Text.Json.Serialization;

namespace Identity.API.Features.Auth.LoginUser;

public record LoginUserCommand(string Username, string Password) : ICommand<LoginResponse>;

public class LoginResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; } = null!;
    
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; init; }
    
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; init; } = null!;
}