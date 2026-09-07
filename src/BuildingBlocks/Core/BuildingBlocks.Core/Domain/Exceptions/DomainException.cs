using BuildingBlocks.Core.Results;

namespace BuildingBlocks.Core.Domain.Exceptions;

public class DomainException(Error error) : Exception(error.Description)
{
    public Error Error { get; } = error;
}