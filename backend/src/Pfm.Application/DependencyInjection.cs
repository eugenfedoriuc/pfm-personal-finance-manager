using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Pfm.Application.Budgets;
using Pfm.Application.Categories;
using Pfm.Application.Transactions;

namespace Pfm.Application;

public static class DependencyInjection
{
    /// <summary>Registers the application services and their validators.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CategoryService>();
        services.AddScoped<TransactionService>();
        services.AddScoped<BudgetService>();

        services.AddSingleton<IValidator<CreateCategoryRequest>, CreateCategoryRequestValidator>();
        services.AddSingleton<IValidator<UpdateCategoryRequest>, UpdateCategoryRequestValidator>();
        services.AddSingleton<IValidator<TransactionRequest>, TransactionRequestValidator>();
        services.AddSingleton<IValidator<UpsertBudgetRequest>, UpsertBudgetRequestValidator>();

        return services;
    }
}
