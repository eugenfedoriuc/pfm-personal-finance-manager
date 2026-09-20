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
        var (statusCode, title, detail) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Nicht gefunden", exception.Message),
            ConflictException => (StatusCodes.Status409Conflict, "Konflikt", exception.Message),
            _ => (StatusCodes.Status500InternalServerError, "Unerwarteter Fehler",
                "Die Anfrage konnte nicht verarbeitet werden.")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception while processing {Method} {Path}.",
                httpContext.Request.Method, httpContext.Request.Path);
        }
        else
        {
            logger.LogInformation("{Method} {Path} failed with {StatusCode}: {Detail}",
                httpContext.Request.Method, httpContext.Request.Path, statusCode, detail);
        }

        httpContext.Response.StatusCode = statusCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail
            }
        });
    }
}
