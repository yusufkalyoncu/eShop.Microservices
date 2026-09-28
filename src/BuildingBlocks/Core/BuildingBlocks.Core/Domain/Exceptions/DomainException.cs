using BuildingBlocks.Core.Results;
using BuildingBlocks.Core.Exceptions;

namespace BuildingBlocks.Core.Domain.Exceptions;

public class DomainException(Error error) : CustomException(error);