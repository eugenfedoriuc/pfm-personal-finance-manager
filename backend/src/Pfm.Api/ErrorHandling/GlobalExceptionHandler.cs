using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Pfm.Domain.Exceptions;

namespace Pfm.Api.ErrorHandling;

/// <summary>
/// Translates domain exceptions into RFC 7807 responses. Anything unexpected becomes a 500 with a
/// generic detail, so that internal messages never leak to the client.
/// </summary>
internal sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails = Describe(exception);
        var statusCode = problemDetails.Status!.Value;

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception while processing {Method} {Path}.",
                httpContext.Request.Method, httpContext.Request.Path);
        }
        else
        {
            logger.LogInformation("{Method} {Path} failed with {StatusCode}: {Message}",
                httpContext.Request.Method, httpContext.Request.Path, statusCode, exception.Message);
        }

        httpContext.Response.StatusCode = statusCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails
        });
    }

    private static ProblemDetails Describe(Exception exception) => exception switch
    {
        ValidationException validation => new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = ValidationProblemFactory.Title,
            // Same key/messages shape as a ValidationProblemDetails, so the UI maps it the same way.
            Extensions =
            {
                ["errors"] = new Dictionary<string, string[]>
                {
                    [ValidationProblemFactory.ToCamelCase(validation.PropertyName)] = [validation.Message]
                }
            }
        },
        NotFoundException => new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Nicht gefunden",
            Detail = exception.Message
        },
        ConflictException => new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Konflikt",
            Detail = exception.Message
        },
        _ => new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Unerwarteter Fehler",
            Detail = "Die Anfrage konnte nicht verarbeitet werden."
        }
    };
}
