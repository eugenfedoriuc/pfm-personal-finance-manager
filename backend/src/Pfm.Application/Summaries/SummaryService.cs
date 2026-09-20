using Pfm.Application.Abstractions;
using Pfm.Application.Common;
using Pfm.Application.Transactions;

namespace Pfm.Application.Summaries;

/// <summary>
/// Loads what a month consists of and hands it to <see cref="MonthlySummaryCalculator"/>. All the
/// arithmetic lives in the calculator; this type only does I/O.
/// </summary>
public sealed class SummaryService(
    ITransactionRepository transactions,
    IBudgetRepository budgets,
    ICategoryRepository categories)
{
    public async Task<MonthlySummaryResponse> GetAsync(MonthRange month, CancellationToken cancellationToken)
    {
        var monthlyTransactions = await transactions.GetAsync(
            new TransactionFilter(month, Type: null, CategoryId: null),
            cancellationToken);

        var monthlyBudgets = await budgets.GetForMonthAsync(month, cancellationToken);
        var allCategories = (await categories.GetAllAsync(cancellationToken)).ToDictionary(category => category.Id);

        return MonthlySummaryCalculator.Calculate(month, monthlyTransactions, monthlyBudgets, allCategories);
    }
}
