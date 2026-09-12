using Microsoft.AspNetCore.Builder;

namespace BuildingBlocks.Web.Security;

public static class AuthorizationExtensions
{
    public static TBuilder RequireRoles<TBuilder>(this TBuilder builder, params string[] roles) 
        where TBuilder : IEndpointConventionBuilder
    {
        return builder.RequireAuthorization(policy => policy.RequireRole(roles));
    }
}