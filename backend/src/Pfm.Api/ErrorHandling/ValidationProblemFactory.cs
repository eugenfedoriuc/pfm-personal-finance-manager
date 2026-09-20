using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Pfm.Api.ErrorHandling;

/// <summary>
/// Builds the 400 response for both sources of invalid input — model binding and
/// FluentValidation — so that the client always sees the same shape and the same title.
/// </summary>
internal static class ValidationProblemFactory
{
    private const string Title = "Ungültige Eingabe";

    public static IActionResult Create(HttpContext httpContext, ModelStateDictionary modelState)
    {
        var factory = httpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();

        var problemDetails = factory.CreateValidationProblemDetails(
            httpContext,
            modelState,
            StatusCodes.Status400BadRequest,
            title: Title);

        return new BadRequestObjectResult(problemDetails);
    }
}
