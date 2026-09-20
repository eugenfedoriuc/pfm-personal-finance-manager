using Pfm.Application.Abstractions;
using Pfm.Application.Categories;
using Pfm.Application.Common;
using Pfm.Domain;
using Pfm.Domain.Entities;
using Pfm.Domain.Exceptions;

namespace Pfm.Application.Budgets;

/// <summary>
/// Use cases for budgets. Enforces that only expense categories can be budgeted
/// (business rule 2).
/// </summary>
public sealed class BudgetService(IBudgetRepository budgets, ICategoryRepository categories)
{
    public async Task<IReadOnlyList<BudgetResponse>> GetForMonthAsync(
        MonthRange month,
        CancellationToken cancellationToken)
    {
        var stored = await budgets.GetForMonthAsync(month, cancellationToken);
        if (stored.Count == 0)
        {
            return [];
        }

        var byId = (await categories.GetAllAsync(cancellationToken)).ToDictionary(category => category.Id);
        return [.. stored.Select(budget => ToResponse(budget, byId[budget.CategoryId]))];
    }

    public async Task<BudgetResponse> UpsertAsync(UpsertBudgetRequest request, CancellationToken cancellationToken)
    {
        var month = MonthRange.Of(request.Year, request.Month);
        var category = await ResolveCategoryAsync(request.CategoryId, cancellationToken);

        var stored = await budgets.UpsertAsync(
            new Budget
            {
                CategoryId = category.Id,
                Year = month.Year,
                Month = month.Month,
                Limit = request.Limit
            },
            cancellationToken);

        return ToResponse(stored, category);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken)
    {
        if (!await budgets.DeleteAsync(id, cancellationToken))
        {
            throw new NotFoundException($"Das Budget \"{id}\" wurde nicht gefunden.");
        }
    }

    private async Task<Category> ResolveCategoryAsync(string categoryId, CancellationToken cancellationToken)
    {
        var category = await categories.GetByIdAsync(categoryId, cancellationToken)
            ?? throw new ValidationException("categoryId", "Die ausgewählte Kategorie existiert nicht.");

        if (category.Type != TransactionType.Expense)
        {
            throw new ValidationException(
                "categoryId",
                $"Für \"{category.Name}\" kann kein Budget gesetzt werden, weil es eine Einnahmenkategorie ist.");
        }

        return category;
    }

    private static BudgetResponse ToResponse(Budget budget, Category category) =>
        new(budget.Id, CategoryResponse.From(category), budget.Year, budget.Month, budget.Limit);
}
