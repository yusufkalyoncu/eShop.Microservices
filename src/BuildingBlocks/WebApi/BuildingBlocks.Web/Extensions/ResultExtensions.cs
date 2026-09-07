using System.Text.Json;
using BuildingBlocks.Core.Results;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Web.Extensions;

public static class ResultExtensions
{
    public static IResult Match(this Result result) =>
        result.IsSuccess
            ? Results.NoContent()
            : result.ToProblemDetails();

    public static IResult Match<T>(this Result<T> result) =>
        result.IsSuccess
            ? Results.Ok(result.Data)
            : result.ToProblemDetails();

    public static IResult Match<T, TResponse>(this Result<T> result, Func<T, TResponse> mapper) =>
        result.IsSuccess
            ? Results.Ok(mapper(result.Data))
            : result.ToProblemDetails();

    private static IResult ToProblemDetails(this Result result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("Cannot convert success result to problem details");
        }

        return Results.Problem(
            title: GetTitle(result.Error),
            detail: result.Error.Description,
            type: GetType(result.Error.Type),
            statusCode: GetStatusCode(result.Error.Type),
            extensions: GetErrors(result));

        static string GetTitle(Error error) =>
            error.Type switch
            {
                ErrorType.BadRequest => "Bad Request",
                ErrorType.Unauthorized => "Unauthorized",
                ErrorType.Forbidden => "Forbidden",
                ErrorType.NotFound => "Not Found",
                ErrorType.Conflict => "Conflict",
                ErrorType.TooManyRequests => "Too Many Requests",
                _ => "Server Failure"
            };

        static string GetType(ErrorType errorType) =>
            errorType switch
            {
                ErrorType.BadRequest => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                ErrorType.Unauthorized => "https://tools.ietf.org/html/rfc7235#section-3.1",
                ErrorType.Forbidden => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                ErrorType.NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                ErrorType.Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
                _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
            };

        static int GetStatusCode(ErrorType errorType) =>
            errorType switch
            {
                ErrorType.BadRequest => StatusCodes.Status400BadRequest,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.TooManyRequests => StatusCodes.Status429TooManyRequests,
                _ => StatusCodes.Status500InternalServerError
            };

        static Dictionary<string, object?>? GetErrors(Result result)
        {
            var errors = result.Error switch
            {
                ValidationError v => v.Errors
                    .Select(e => new ErrorDetail(NormalizeField(e.Field), e.Description))
                    .ToArray(),

                var err when NormalizeField(err.Field) is { } field =>
                    [new ErrorDetail(field, err.Description)],

                _ => null
            };

            return errors is not null
                ? new Dictionary<string, object?> { ["errors"] = errors }
                : null;
        }
    }

    private record ErrorDetail(string? Field, string Message);

    private static string? NormalizeField(string? field)
    {
        if (string.IsNullOrWhiteSpace(field))
        {
            return null;
        }

        return string.Join(
            ".",
            field.Split('.')
                .Select(static segment =>
                {
                    var indexStart = segment.IndexOf('[');
                    if (indexStart < 0)
                    {
                        return JsonNamingPolicy.CamelCase.ConvertName(segment);
                    }

                    var propertyName = segment[..indexStart];
                    var suffix = segment[indexStart..];

                    return string.IsNullOrEmpty(propertyName)
                        ? segment
                        : $"{JsonNamingPolicy.CamelCase.ConvertName(propertyName)}{suffix}";
                }));
    }
}