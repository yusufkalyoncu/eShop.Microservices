using System.Text.Json;
using BuildingBlocks.Core.Caching;
using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Core.Results;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Application.Behaviors;

internal static class CachingDecorator
{
    internal sealed class QueryHandler<TQuery, TResponse>(
        IQueryHandler<TQuery, TResponse> innerHandler,
        IDistributedCache cache,
        ILogger<QueryHandler<TQuery, TResponse>> logger)
        : IQueryHandler<TQuery, TResponse>
        where TQuery : IQuery<TResponse>
    {
        public async Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken)
        {
            if ((object)query is not ICacheableQuery cacheableQuery)
            {
                return await innerHandler.Handle(query, cancellationToken);
            }

            string cacheKey = cacheableQuery.CacheKey;
            string? cachedData = await cache.GetStringAsync(cacheKey, cancellationToken);

            if (!string.IsNullOrEmpty(cachedData))
            {
                logger.LogInformation("Fetched from Cache -> '{CacheKey}'", cacheKey);
                var cachedResponse = JsonSerializer.Deserialize<TResponse>(cachedData);
                if (cachedResponse is not null)
                {
                    return Result.Success(cachedResponse);
                }
            }

            var result = await innerHandler.Handle(query, cancellationToken);

            if (result is { IsSuccess: true, Data: not null })
            {
                var expiration = cacheableQuery.Expiration ?? TimeSpan.FromMinutes(5);
                var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiration };

                string serializedData = JsonSerializer.Serialize(result.Data);
                await cache.SetStringAsync(cacheKey, serializedData, options, cancellationToken);
                
                logger.LogInformation("Added to Cache -> '{CacheKey}'", cacheKey);
            }

            return result;
        }
    }

    internal sealed class CommandHandler<TCommand, TResponse>(
        ICommandHandler<TCommand, TResponse> innerHandler,
        IDistributedCache cache,
        ILogger<CommandHandler<TCommand, TResponse>> logger)
        : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        public async Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken)
        {
            var result = await innerHandler.Handle(command, cancellationToken);

            if (result.IsSuccess && (object)command is ICacheInvalidatorCommand invalidatorCommand)
            {
                foreach (var cacheKey in invalidatorCommand.CacheKeys)
                {
                    await cache.RemoveAsync(cacheKey, cancellationToken);
                    logger.LogInformation("Removed from Cache -> '{CacheKey}'", cacheKey);
                }
            }

            return result;
        }
    }

    internal sealed class CommandBaseHandler<TCommand>(
        ICommandHandler<TCommand> innerHandler,
        IDistributedCache cache,
        ILogger<CommandBaseHandler<TCommand>> logger)
        : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        public async Task<Result> Handle(TCommand command, CancellationToken cancellationToken)
        {
            var result = await innerHandler.Handle(command, cancellationToken);

            if (result.IsSuccess && (object)command is ICacheInvalidatorCommand invalidatorCommand)
            {
                foreach (var cacheKey in invalidatorCommand.CacheKeys)
                {
                    await cache.RemoveAsync(cacheKey, cancellationToken);
                    logger.LogInformation("Removed from Cache -> '{CacheKey}'", cacheKey);
                }
            }

            return result;
        }
    }
}
