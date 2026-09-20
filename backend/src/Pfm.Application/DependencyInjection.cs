using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Pfm.Application.Categories;

namespace Pfm.Application;

public static class DependencyInjection
{
    /// <summary>Registers the application services and their validators.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CategoryService>();

        services.AddSingleton<IValidator<CreateCategoryRequest>, CreateCategoryRequestValidator>();
        services.AddSingleton<IValidator<UpdateCategoryRequest>, UpdateCategoryRequestValidator>();

        return services;
    }
}
