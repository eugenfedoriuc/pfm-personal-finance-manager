using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Pfm.Api.ErrorHandling;

/// <summary>
/// Builds the 400 response for every source of invalid input — model binding, FluentValidation and
/// the domain rules — so that the client always sees the same shape and the same title.
/// </summary>
internal static class ValidationProblemFactory
{
    public const string Title = "Ungültige Eingabe";

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

    /// <summary>Aligns the error keys with the camelCase property names used in JSON and in the UI forms.</summary>
    public static string ToCamelCase(string propertyName) =>
        propertyName.Length == 0 ? propertyName : char.ToLowerInvariant(propertyName[0]) + propertyName[1..];
}
