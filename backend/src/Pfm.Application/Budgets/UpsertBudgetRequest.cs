namespace Pfm.Application.Budgets;

/// <summary>
/// Payload for <c>PUT /api/budgets</c>. The budget is identified by category, year and month, so
/// the same request either creates the limit or replaces it.
/// </summary>
/// <param name="CategoryId">Id of an existing expense category.</param>
/// <param name="Year">Calendar year of the budget.</param>
/// <param name="Month">Calendar month of the budget, 1 to 12.</param>
/// <param name="Limit">Positive spending limit, at most two decimal places.</param>
public sealed record UpsertBudgetRequest(string CategoryId, int Year, int Month, decimal Limit);
