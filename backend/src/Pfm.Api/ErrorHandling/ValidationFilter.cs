using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Pfm.Api.ErrorHandling;

/// <summary>
/// Runs the FluentValidation validator of every action argument that has one. Model binding has
/// already succeeded at this point, so this filter only reports rule violations.
/// </summary>
internal sealed class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var errors = new ModelStateDictionary();

        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
            {
                continue;
            }

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (context.HttpContext.RequestServices.GetService(validatorType) is not IValidator validator)
            {
                continue;
            }

            var result = await validator.ValidateAsync(
                new ValidationContext<object>(argument),
                context.HttpContext.RequestAborted);

            foreach (var failure in result.Errors)
            {
                errors.AddModelError(ToCamelCase(failure.PropertyName), failure.ErrorMessage);
            }
        }

        if (!errors.IsValid)
        {
            context.Result = ValidationProblemFactory.Create(context.HttpContext, errors);
            return;
        }

        await next();
    }

    /// <summary>Aligns the error keys with the camelCase property names used in JSON and in the UI forms.</summary>
    private static string ToCamelCase(string propertyName) =>
        propertyName.Length == 0 ? propertyName : char.ToLowerInvariant(propertyName[0]) + propertyName[1..];
}
