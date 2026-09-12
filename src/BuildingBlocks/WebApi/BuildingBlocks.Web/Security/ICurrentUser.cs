using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Web.Security;

public interface ICurrentUser
{
    string? Id { get; }
    string? Name { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
    IEnumerable<Claim> Claims { get; }
}

internal sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public string? Id => Principal?.FindFirstValue(ClaimTypes.NameIdentifier) ?? Principal?.FindFirstValue("sub");
    
    public string? Name => Principal?.FindFirstValue(ClaimTypes.Name) ?? Principal?.FindFirstValue("preferred_username");
    
    public string? Email => Principal?.FindFirstValue(ClaimTypes.Email);
    
    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;
    
    public IEnumerable<Claim> Claims => Principal?.Claims ?? Enumerable.Empty<Claim>();
}