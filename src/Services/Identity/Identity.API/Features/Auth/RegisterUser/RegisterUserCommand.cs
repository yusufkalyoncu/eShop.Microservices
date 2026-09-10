using BuildingBlocks.Core.CQRS;

using Identity.API.Features.Auth.LoginUser;

namespace Identity.API.Features.Auth.RegisterUser;

public record RegisterUserCommand(string Username, string Email, string FirstName, string LastName, string Password) : ICommand<LoginResponse>;