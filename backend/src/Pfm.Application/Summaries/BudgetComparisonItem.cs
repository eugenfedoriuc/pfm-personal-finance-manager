namespace Pfm.Application.Summaries;

/// <summary>
/// Budget against reality for one expense category in a month. Categories appear here when they
/// have a budget, when they have spending, or both — so <paramref name="Limit"/> is null for a
/// category that was spent on without a budget, and <paramref name="Spent"/> is zero for a budget
/// that was not touched.
/// </summary>
/// <param name="CategoryId">Id of the category.</param>
/// <param name="CategoryName">Name of the category.</param>
/// <param name="Limit">The budgeted limit, or null if no budget is set for this month.</param>
/// <param name="Spent">Sum of the category's expenses in the month.</param>
/// <param name="Remaining">
/// <c>Limit - Spent</c>, negative once the budget is exceeded. Null without a budget.
/// </param>
/// <param name="IsOverBudget"><c>Spent &gt; Limit</c>. False without a budget.</param>
/// <param name="OverBy"><c>max(0, Spent - Limit)</c>. Null without a budget.</param>
public sealed record BudgetComparisonItem(
    string CategoryId,
    string CategoryName,
    decimal? Limit,
    decimal Spent,
    decimal? Remaining,
    bool IsOverBudget,
    decimal? OverBy);
