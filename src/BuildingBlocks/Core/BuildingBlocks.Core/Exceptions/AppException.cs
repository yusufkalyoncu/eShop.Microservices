using BuildingBlocks.Core.Results;

namespace BuildingBlocks.Core.Exceptions;

public class AppException(Error error) : CustomException(error);