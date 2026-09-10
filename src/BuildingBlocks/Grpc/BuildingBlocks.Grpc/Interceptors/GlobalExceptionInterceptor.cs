using BuildingBlocks.Core.Domain.Exceptions;
using BuildingBlocks.Core.Results;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Grpc.Interceptors;

public class GlobalExceptionInterceptor(ILogger<GlobalExceptionInterceptor> logger) : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (Exception ex)
        {
            HandleException(ex);
            throw; // This line should ideally not be reached because HandleException throws RpcException, but compiler might complain if we don't return or throw.
        }
    }

    private void HandleException(Exception exception)
    {
        logger.LogError(exception, "An error occurred while processing the gRPC request.");

        if (exception is DomainException domainException)
        {
            var statusCode = domainException.Error.Type switch
            {
                ErrorType.NotFound => StatusCode.NotFound,
                ErrorType.BadRequest => StatusCode.InvalidArgument,
                ErrorType.Conflict => StatusCode.AlreadyExists,
                ErrorType.Unauthorized => StatusCode.Unauthenticated,
                ErrorType.Forbidden => StatusCode.PermissionDenied,
                _ => StatusCode.Internal
            };

            var metadata = new Metadata
            {
                { "error-code", domainException.Error.Code }
            };

            throw new RpcException(new Status(statusCode, domainException.Error.Description), metadata);
        }

        throw new RpcException(new Status(StatusCode.Internal, "An unexpected error occurred."));
    }
}