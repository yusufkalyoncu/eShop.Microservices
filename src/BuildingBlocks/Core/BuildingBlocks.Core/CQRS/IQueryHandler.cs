using BuildingBlocks.Core.Results;

namespace BuildingBlocks.Core.CQRS;

public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    Task<Result<TResponse>> Handle(TQuery request, CancellationToken cancellationToken);
}