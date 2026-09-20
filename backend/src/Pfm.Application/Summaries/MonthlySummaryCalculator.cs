using Pfm.Application.Common;
using Pfm.Domain;
using Pfm.Domain.Entities;

namespace Pfm.Application.Summaries;

/// <summary>
/// Turns the transactions and budgets of one month into the monthly summary. Deliberately pure: no
/// I/O, no clock, no culture-dependent ordering — the same input always produces the same output,
/// which is what makes the arithmetic worth unit-testing.
/// </summary>
public static class MonthlySummaryCalculator
{
    /// <param name="month">The month being summarised.</param>
    /// <param name="transactions">The transactions of that month, in any order.</param>
    /// <param name="budgets">The budgets of that month, at most one per category.</param>
    /// <param name="categories">Every category referenced by the transactions and budgets.</param>
    public static MonthlySummaryResponse Calculate(
        MonthRange month,
        IReadOnlyList<Transaction> transactions,
        IReadOnlyList<Budget> budgets,
        IReadOnlyDictionary<string, Category> categories)
    {
        var income = transactions.Where(transaction => transaction.Type == TransactionType.Income).ToList();
        var expenses = transactions.Where(transaction => transaction.Type == TransactionType.Expense).ToList();

        var totalIncome = income.Sum(transaction => transaction.Amount);
        var totalExpenses = expenses.Sum(transaction => transaction.Amount);

        var expenseBreakdown = Breakdown(expenses, totalExpenses, categories);

        return new MonthlySummaryResponse(
            month.Year,
            month.Month,
            totalIncome,
            totalExpenses,
            totalIncome - totalExpenses,
            Breakdown(income, totalIncome, categories),
            expenseBreakdown,
            CompareBudgets(budgets, expenseBreakdown, categories),
            // The breakdown is already ordered by amount, ties alphabetically (business rule 5).
            expenseBreakdown.FirstOrDefault(),
            DailyExpenses(month, expenses));
    }

    private static IReadOnlyList<CategoryBreakdownItem> Breakdown(
        IReadOnlyList<Transaction> transactions,
        decimal total,
        IReadOnlyDictionary<string, Category> categories) =>
        [.. transactions
            .GroupBy(transaction => transaction.CategoryId)
            .Select(perCategory =>
            {
                var category = categories[perCategory.Key];
                var amount = perCategory.Sum(transaction => transaction.Amount);

                return new CategoryBreakdownItem(
                    category.Id,
                    category.Name,
                    category.Icon,
                    category.Color,
                    amount,
                    perCategory.Count(),
                    Percentage(amount, total));
            })
            .OrderByDescending(item => item.Amount)
            .ThenBy(item => item.Name, StringComparer.InvariantCulture)];

    private static IReadOnlyList<BudgetComparisonItem> CompareBudgets(
        IReadOnlyList<Budget> budgets,
        IReadOnlyList<CategoryBreakdownItem> expenseBreakdown,
        IReadOnlyDictionary<string, Category> categories)
    {
        var spentPerCategory = expenseBreakdown.ToDictionary(item => item.CategoryId, item => item.Amount);
        var limitPerCategory = budgets.ToDictionary(budget => budget.CategoryId, budget => budget.Limit);

        // Business rule 6: a budget without spending and spending without a budget both show up.
        return [.. spentPerCategory.Keys
            .Union(limitPerCategory.Keys)
            .Select(categoryId =>
            {
                var category = categories[categoryId];
                var spent = spentPerCategory.GetValueOrDefault(categoryId);
                decimal? limit = limitPerCategory.TryGetValue(categoryId, out var stored) ? stored : null;

                return new BudgetComparisonItem(
                    category.Id,
                    category.Name,
                    limit,
                    spent,
                    limit - spent,
                    limit is not null && spent > limit,
                    limit is null ? null : Math.Max(0m, spent - limit.Value));
            })
            .OrderBy(item => item.CategoryName, StringComparer.InvariantCulture)];
    }

    private static IReadOnlyList<DailyExpenseItem> DailyExpenses(
        MonthRange month,
        IReadOnlyList<Transaction> expenses)
    {
        var perDay = expenses
            .GroupBy(transaction => transaction.Date)
            .ToDictionary(group => group.Key, group => group.Sum(transaction => transaction.Amount));

        var days = new List<DailyExpenseItem>(month.DayCount);
        var cumulative = 0m;

        for (var day = month.Start; day < month.EndExclusive; day = day.AddDays(1))
        {
            var amount = perDay.GetValueOrDefault(day);
            cumulative += amount;
            days.Add(new DailyExpenseItem(day, amount, cumulative));
        }

        return days;
    }

    private static decimal Percentage(decimal amount, decimal total) =>
        total == 0 ? 0m : decimal.Round(amount / total * 100m, 2);
}
