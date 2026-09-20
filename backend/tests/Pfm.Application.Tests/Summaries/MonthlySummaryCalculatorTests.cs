using Pfm.Application.Common;
using Pfm.Application.Summaries;
using Pfm.Domain;
using Pfm.Domain.Entities;

namespace Pfm.Application.Tests.Summaries;

public sealed class MonthlySummaryCalculatorTests
{
    private const string Miete = TestData.ExpenseCategoryId;
    private const string Gehalt = TestData.IncomeCategoryId;
    private const string Lebensmittel = "aaaaaaaaaaaaaaaaaaaaaaa3";
    private const string Freizeit = "aaaaaaaaaaaaaaaaaaaaaaa4";

    private static readonly MonthRange March2025 = MonthRange.Of(2025, 3);

    private static readonly Dictionary<string, Category> Categories = new()
    {
        [Miete] = TestData.Category(Miete, "Miete"),
        [Gehalt] = TestData.Category(Gehalt, "Gehalt", TransactionType.Income),
        [Lebensmittel] = TestData.Category(Lebensmittel, "Lebensmittel"),
        [Freizeit] = TestData.Category(Freizeit, "Freizeit")
    };

    [Fact]
    public void EmptyMonth_HasZeroTotalsAndNoTopCategory()
    {
        var summary = Calculate();

        Assert.Equal(2025, summary.Year);
        Assert.Equal(3, summary.Month);
        Assert.Equal(0m, summary.TotalIncome);
        Assert.Equal(0m, summary.TotalExpenses);
        Assert.Equal(0m, summary.Balance);
        Assert.Empty(summary.IncomeBreakdown);
        Assert.Empty(summary.ExpenseBreakdown);
        Assert.Empty(summary.BudgetComparison);
        Assert.Null(summary.TopExpenseCategory);
    }

    [Fact]
    public void EmptyMonth_StillReportsEveryDayWithZeroExpenses()
    {
        var summary = Calculate();

        Assert.Equal(31, summary.DailyExpenses.Count);
        Assert.All(summary.DailyExpenses, day =>
        {
            Assert.Equal(0m, day.Amount);
            Assert.Equal(0m, day.Cumulative);
        });
    }

    [Fact]
    public void Totals_AreTheSumsOfIncomeAndExpenses()
    {
        var summary = Calculate(transactions:
        [
            Income(Gehalt, 2500m, 31),
            Expense(Miete, 900m, 1),
            Expense(Lebensmittel, 900m, 15)
        ]);

        Assert.Equal(2500m, summary.TotalIncome);
        Assert.Equal(1800m, summary.TotalExpenses);
        Assert.Equal(700m, summary.Balance);
    }

    [Fact]
    public void Balance_IsNegativeWhenMoreWasSpentThanEarned()
    {
        var summary = Calculate(transactions: [Income(Gehalt, 100m, 1), Expense(Miete, 250.50m, 2)]);

        Assert.Equal(-150.50m, summary.Balance);
    }

    [Fact]
    public void Breakdown_GroupsPerCategoryAndCountsTransactions()
    {
        var summary = Calculate(transactions:
        [
            Expense(Lebensmittel, 30m, 2),
            Expense(Lebensmittel, 20m, 7),
            Expense(Miete, 900m, 1)
        ]);

        Assert.Collection(
            summary.ExpenseBreakdown,
            item =>
            {
                Assert.Equal("Miete", item.Name);
                Assert.Equal(900m, item.Amount);
                Assert.Equal(1, item.TransactionCount);
            },
            item =>
            {
                Assert.Equal("Lebensmittel", item.Name);
                Assert.Equal(50m, item.Amount);
                Assert.Equal(2, item.TransactionCount);
            });
    }

    [Fact]
    public void Breakdown_ReportsTheShareOfTheRespectiveTotal()
    {
        var summary = Calculate(transactions:
        [
            Income(Gehalt, 1000m, 1),
            Expense(Miete, 750m, 1),
            Expense(Lebensmittel, 250m, 2)
        ]);

        Assert.Equal(100m, Assert.Single(summary.IncomeBreakdown).Percentage);
        Assert.Equal(75m, summary.ExpenseBreakdown[0].Percentage);
        Assert.Equal(25m, summary.ExpenseBreakdown[1].Percentage);
    }

    [Fact]
    public void Breakdown_RoundsThePercentageToTwoDecimals()
    {
        var summary = Calculate(transactions: [Expense(Miete, 1m, 1), Expense(Lebensmittel, 2m, 2)]);

        Assert.Equal(66.67m, summary.ExpenseBreakdown[0].Percentage);
        Assert.Equal(33.33m, summary.ExpenseBreakdown[1].Percentage);
    }

    [Fact]
    public void TopExpenseCategory_IsTheOneWithTheHighestTotal()
    {
        var summary = Calculate(transactions:
        [
            Expense(Lebensmittel, 100m, 2),
            Expense(Miete, 900m, 1),
            Expense(Freizeit, 50m, 3)
        ]);

        Assert.NotNull(summary.TopExpenseCategory);
        Assert.Equal("Miete", summary.TopExpenseCategory.Name);
        Assert.Equal(900m, summary.TopExpenseCategory.Amount);
    }

    [Fact]
    public void TopExpenseCategory_BreaksTiesAlphabetically()
    {
        var summary = Calculate(transactions:
        [
            Expense(Miete, 500m, 1),
            Expense(Freizeit, 500m, 2),
            Expense(Lebensmittel, 500m, 3)
        ]);

        Assert.NotNull(summary.TopExpenseCategory);
        Assert.Equal("Freizeit", summary.TopExpenseCategory.Name);
        Assert.Equal(["Freizeit", "Lebensmittel", "Miete"], summary.ExpenseBreakdown.Select(item => item.Name));
    }

    [Fact]
    public void TopExpenseCategory_IgnoresIncome()
    {
        var summary = Calculate(transactions: [Income(Gehalt, 5000m, 1)]);

        Assert.Null(summary.TopExpenseCategory);
        Assert.Single(summary.IncomeBreakdown);
    }

    [Fact]
    public void BudgetComparison_ListsABudgetThatWasNotSpentOn()
    {
        var summary = Calculate(budgets: [TestData.Budget(categoryId: Miete, limit: 300m)]);

        var item = Assert.Single(summary.BudgetComparison);
        Assert.Equal("Miete", item.CategoryName);
        Assert.Equal(300m, item.Limit);
        Assert.Equal(0m, item.Spent);
        Assert.Equal(300m, item.Remaining);
        Assert.False(item.IsOverBudget);
        Assert.Equal(0m, item.OverBy);
    }

    [Fact]
    public void BudgetComparison_ListsSpendingWithoutABudget()
    {
        var summary = Calculate(transactions: [Expense(Miete, 900m, 1)]);

        var item = Assert.Single(summary.BudgetComparison);
        Assert.Null(item.Limit);
        Assert.Equal(900m, item.Spent);
        Assert.Null(item.Remaining);
        Assert.False(item.IsOverBudget);
        Assert.Null(item.OverBy);
    }

    [Fact]
    public void BudgetComparison_TreatsSpendingExactlyAtTheLimitAsNotExceeded()
    {
        var summary = Calculate(
            transactions: [Expense(Miete, 300m, 4)],
            budgets: [TestData.Budget(categoryId: Miete, limit: 300m)]);

        var item = Assert.Single(summary.BudgetComparison);
        Assert.False(item.IsOverBudget);
        Assert.Equal(0m, item.Remaining);
        Assert.Equal(0m, item.OverBy);
    }

    [Fact]
    public void BudgetComparison_ReportsTheOverrun()
    {
        var summary = Calculate(
            transactions: [Expense(Miete, 310m, 4)],
            budgets: [TestData.Budget(categoryId: Miete, limit: 300m)]);

        var item = Assert.Single(summary.BudgetComparison);
        Assert.True(item.IsOverBudget);
        Assert.Equal(-10m, item.Remaining);
        Assert.Equal(10m, item.OverBy);
    }

    [Fact]
    public void BudgetComparison_CombinesBudgetedAndUnbudgetedCategoriesByName()
    {
        var summary = Calculate(
            transactions: [Expense(Miete, 900m, 1), Expense(Freizeit, 40m, 2)],
            budgets:
            [
                TestData.Budget(categoryId: Miete, limit: 800m),
                TestData.Budget("ccccccccccccccccccccccc2", Lebensmittel, limit: 250m)
            ]);

        Assert.Equal(
            ["Freizeit", "Lebensmittel", "Miete"],
            summary.BudgetComparison.Select(item => item.CategoryName));

        Assert.Null(summary.BudgetComparison[0].Limit);
        Assert.Equal(0m, summary.BudgetComparison[1].Spent);
        Assert.True(summary.BudgetComparison[2].IsOverBudget);
    }

    [Fact]
    public void DailyExpenses_AccumulatesAcrossTheMonthAndIgnoresIncome()
    {
        var summary = Calculate(transactions:
        [
            Expense(Miete, 900m, 1),
            Expense(Lebensmittel, 40m, 15),
            Income(Gehalt, 2500m, 15)
        ]);

        Assert.Equal(new DateOnly(2025, 3, 1), summary.DailyExpenses[0].Date);
        Assert.Equal(900m, summary.DailyExpenses[0].Cumulative);
        Assert.Equal(0m, summary.DailyExpenses[1].Amount);
        Assert.Equal(900m, summary.DailyExpenses[1].Cumulative);
        Assert.Equal(40m, summary.DailyExpenses[14].Amount);
        Assert.Equal(940m, summary.DailyExpenses[14].Cumulative);
        Assert.Equal(940m, summary.DailyExpenses[^1].Cumulative);
    }

    [Fact]
    public void DailyExpenses_SumsSeveralTransactionsOnTheSameDay()
    {
        var summary = Calculate(transactions:
        [
            Expense(Lebensmittel, 12.50m, 7),
            Expense(Freizeit, 7.25m, 7),
            Expense(Miete, 0.25m, 7)
        ]);

        Assert.Equal(20m, summary.DailyExpenses[6].Amount);
        Assert.Equal(20m, summary.DailyExpenses[6].Cumulative);
    }

    [Fact]
    public void DailyExpenses_CoversTheLeapDayInFebruary()
    {
        var summary = MonthlySummaryCalculator.Calculate(
            MonthRange.Of(2024, 2),
            [TestData.Transaction(date: "2024-02-29", amount: 10m, categoryId: Miete)],
            [],
            Categories);

        Assert.Equal(29, summary.DailyExpenses.Count);
        Assert.Equal(new DateOnly(2024, 2, 29), summary.DailyExpenses[^1].Date);
        Assert.Equal(10m, summary.DailyExpenses[^1].Amount);
    }

    [Fact]
    public void DailyExpenses_StopsBeforeTheFirstOfTheNextMonth()
    {
        var summary = MonthlySummaryCalculator.Calculate(MonthRange.Of(2025, 4), [], [], Categories);

        Assert.Equal(30, summary.DailyExpenses.Count);
        Assert.Equal(new DateOnly(2025, 4, 1), summary.DailyExpenses[0].Date);
        Assert.Equal(new DateOnly(2025, 4, 30), summary.DailyExpenses[^1].Date);
    }

    [Fact]
    public void DailyExpenses_RollsOverTheYearBoundaryForDecember()
    {
        var summary = MonthlySummaryCalculator.Calculate(MonthRange.Of(2025, 12), [], [], Categories);

        Assert.Equal(31, summary.DailyExpenses.Count);
        Assert.Equal(new DateOnly(2025, 12, 31), summary.DailyExpenses[^1].Date);
    }

    private static MonthlySummaryResponse Calculate(
        IReadOnlyList<Transaction>? transactions = null,
        IReadOnlyList<Budget>? budgets = null) =>
        MonthlySummaryCalculator.Calculate(March2025, transactions ?? [], budgets ?? [], Categories);

    private static Transaction Expense(string categoryId, decimal amount, int day) =>
        TestData.Transaction(
            id: $"{categoryId}-{day}-{amount}",
            amount: amount,
            date: $"2025-03-{day:00}",
            categoryId: categoryId);

    private static Transaction Income(string categoryId, decimal amount, int day) =>
        TestData.Transaction(
            id: $"{categoryId}-{day}-{amount}",
            type: TransactionType.Income,
            amount: amount,
            date: $"2025-03-{day:00}",
            categoryId: categoryId);
}
