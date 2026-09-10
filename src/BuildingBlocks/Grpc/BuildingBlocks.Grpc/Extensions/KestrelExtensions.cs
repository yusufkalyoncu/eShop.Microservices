using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace BuildingBlocks.Grpc.Extensions;

public static class KestrelExtensions
{
    /// <summary>
    /// Configures Kestrel to use port 8080 for HTTP/1.1 and HTTP/2, 
    /// and port 8081 specifically for HTTP/2 (gRPC without TLS).
    /// </summary>
    public static IWebHostBuilder ConfigureGrpcPorts(this IWebHostBuilder webHostBuilder)
    {
        return webHostBuilder.ConfigureKestrel(options =>
        {
            options.ListenAnyIP(8080, listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
            });
            
            options.ListenAnyIP(8081, listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http2;
            });
        });
    }
}