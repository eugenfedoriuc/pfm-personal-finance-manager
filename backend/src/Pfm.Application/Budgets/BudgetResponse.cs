using Pfm.Application.Categories;

namespace Pfm.Application.Budgets;

/// <summary>
/// A budget as returned by the API, with its category embedded for display. The comparison against
/// what was actually spent is part of the monthly summary, not of this resource.
/// </summary>
public sealed record BudgetResponse(string Id, CategoryResponse Category, int Year, int Month, decimal Limit);
