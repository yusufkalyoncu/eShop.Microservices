using BuildingBlocks.Core.Results;

namespace BuildingBlocks.Core.Exceptions;

public abstract class CustomException(Error error) : Exception(error.Description)
{
    public Error Error { get; } = error;
}