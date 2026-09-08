using Scalar.AspNetCore;

namespace Gateway.API.Extensions;

public static class GatewayDocsExtensions
{
    public static WebApplication UseGatewayDocs(this WebApplication app, Dictionary<string, string> services)
    {
        foreach (var (serviceName, openApiRoute) in services)
        {
            app.MapScalarApiReference($"/scalar/{serviceName}", options =>
            {
                options.AddServer($"http://localhost:5050/{serviceName}-api");
                options.WithTitle($"{serviceName.ToUpper()} API");
                options.WithOpenApiRoutePattern(openApiRoute);
            });
        }

        return app;
    }
}