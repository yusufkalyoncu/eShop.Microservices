namespace BuildingBlocks.Core.Results;

public record Error
{
    #region Properties

    public string Code { get; }
    public string Description { get; }
    public ErrorType Type { get; }
    public string? Field { get; }
    public Dictionary<string, object?>? Args { get; private init; }

    #endregion

    #region Constructor

    internal Error(string code, string description, ErrorType type, string? field = null)
    {
        Code = code;
        Description = description;
        Type = type;
        Field = field;
    }

    #endregion

    #region Equality

    public virtual bool Equals(Error? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Code == other.Code && Description == other.Description && Type == other.Type && Field == other.Field;
    }

    public override int GetHashCode() => HashCode.Combine(Code, Description, Type, Field);

    #endregion

    #region Factory Methods

    private static Error Create(
        string code,
        string description,
        ErrorType type,
        Dictionary<string, object?>? args,
        string? field) =>
        new(code, description, type, field) { Args = args };

    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.None);
    public static readonly Error NullValue = new("General.Null", "Null value was provided.", ErrorType.BadRequest);
    public static readonly Error Unexpected = new("General.Unexpected", "An unexpected error occurred.", ErrorType.InternalServerError);

    public static Error Validation(
        string code,
        string description,
        Dictionary<string, object?>? args = null,
        string? field = null) =>
        Create(code, description, ErrorType.BadRequest, args, field);

    public static Error BadRequest(
        string code,
        string description,
        Dictionary<string, object?>? args = null,
        string? field = null) =>
        Create(code, description, ErrorType.BadRequest, args, field);

    public static Error NotFound(
        string code,
        string description,
        Dictionary<string, object?>? args = null,
        string? field = null) =>
        Create(code, description, ErrorType.NotFound, args, field);

    public static Error Unauthorized(
        string code,
        string description,
        Dictionary<string, object?>? args = null,
        string? field = null) =>
        Create(code, description, ErrorType.Unauthorized, args, field);

    public static Error Forbidden(
        string code,
        string description,
        Dictionary<string, object?>? args = null,
        string? field = null) =>
        Create(code, description, ErrorType.Forbidden, args, field);

    public static Error Conflict(
        string code,
        string description,
        Dictionary<string, object?>? args = null,
        string? field = null) =>
        Create(code, description, ErrorType.Conflict, args, field);

    public static Error TooManyRequests(
        string code,
        string description,
        Dictionary<string, object?>? args = null,
        string? field = null) =>
        Create(code, description, ErrorType.TooManyRequests, args, field);

    public static Error InternalServerError(
        string code,
        string description,
        Dictionary<string, object?>? args = null,
        string? field = null) =>
        Create(code, description, ErrorType.InternalServerError, args, field);

    #endregion
}

public sealed record ValidationError(Error[] Errors) : Error("General.Validation", "One or more validation errors occurred.", ErrorType.BadRequest);