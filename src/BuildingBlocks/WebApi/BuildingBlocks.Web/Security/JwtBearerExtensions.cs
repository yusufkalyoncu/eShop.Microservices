using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace BuildingBlocks.Web.Security;

public static class JwtBearerExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<IdentityOptions>(configuration.GetSection(IdentityOptions.SectionName));
        
        var identityOptions = configuration.GetSection(IdentityOptions.SectionName).Get<IdentityOptions>();

        if (identityOptions != null && !string.IsNullOrEmpty(identityOptions.Authority))
        {
            // In production, RequireHttpsMetadata MUST be true. We read this dynamically if possible, or leave false ONLY for local dev.
            // For industry standard, this should also come from configuration, e.g. identityOptions.RequireHttpsMetadata.
            // For now, we keep it false because Keycloak is HTTP locally.
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = identityOptions.Authority;
                    options.Audience = identityOptions.Audience;
                    options.RequireHttpsMetadata = false; // Dev environment only

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = true,
                        ValidateIssuer = true,
                        ValidIssuers = identityOptions.ValidIssuers ?? [identityOptions.Authority],
                        ValidateLifetime = true,
                        RoleClaimType = ClaimTypes.Role
                    };
                    
                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = context =>
                        {
                            var principal = context.Principal;
                            if (principal?.Identity is ClaimsIdentity identity)
                            {
                                var realmAccessClaim = principal.FindFirst("realm_access")?.Value;
                                if (!string.IsNullOrEmpty(realmAccessClaim))
                                {
                                    try
                                    {
                                        using var json = JsonDocument.Parse(realmAccessClaim);
                                        if (json.RootElement.TryGetProperty("roles", out var rolesElement))
                                        {
                                            foreach (var role in rolesElement.EnumerateArray())
                                            {
                                                var roleValue = role.GetString();
                                                if (!string.IsNullOrEmpty(roleValue))
                                                {
                                                    identity.AddClaim(new Claim(ClaimTypes.Role, roleValue));
                                                }
                                            }
                                        }
                                    }
                                    catch
                                    {
                                        // Ignore parse errors
                                    }
                                }
                            }
                            return Task.CompletedTask;
                        }
                    };
                });
            services.AddAuthorization();
        }

        return services;
    }
    
    public static IServiceCollection AddCurrentUser(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        return services;
    }
}