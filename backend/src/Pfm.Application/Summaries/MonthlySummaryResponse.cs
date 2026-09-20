namespace Pfm.Application.Summaries;

/// <summary>Everything the dashboard needs about one month, in a single response.</summary>
/// <param name="Year">Calendar year of the month.</param>
/// <param name="Month">Calendar month, 1 to 12.</param>
/// <param name="TotalIncome">Sum of all income in the month.</param>
/// <param name="TotalExpenses">Sum of all expenses in the month.</param>
/// <param name="Balance"><c>TotalIncome - TotalExpenses</c>, negative when more was spent than earned.</param>
/// <param name="IncomeBreakdown">Income per category, largest first.</param>
/// <param name="ExpenseBreakdown">Expenses per category, largest first.</param>
/// <param name="BudgetComparison">Budget against actual spending, by category name.</param>
/// <param name="TopExpenseCategory">
/// The category with the highest expenses, or null if nothing was spent.
/// </param>
/// <param name="DailyExpenses">One entry per day of the month, including days without expenses.</param>
public sealed record MonthlySummaryResponse(
    int Year,
    int Month,
    decimal TotalIncome,
    decimal TotalExpenses,
    decimal Balance,
    IReadOnlyList<CategoryBreakdownItem> IncomeBreakdown,
    IReadOnlyList<CategoryBreakdownItem> ExpenseBreakdown,
    IReadOnlyList<BudgetComparisonItem> BudgetComparison,
    CategoryBreakdownItem? TopExpenseCategory,
    IReadOnlyList<DailyExpenseItem> DailyExpenses);
