using NSubstitute;
using Pfm.Application.Abstractions;
using Pfm.Application.Common;
using Pfm.Application.Summaries;
using Pfm.Application.Transactions;
using Pfm.Domain.Entities;

namespace Pfm.Application.Tests.Summaries;

public sealed class SummaryServiceTests
{
    private readonly ITransactionRepository _transactions = Substitute.For<ITransactionRepository>();
    private readonly IBudgetRepository _budgets = Substitute.For<IBudgetRepository>();
    private readonly ICategoryRepository _categories = Substitute.For<ICategoryRepository>();

    [Fact]
    public async Task GetAsync_AsksForTheWholeMonthWithoutNarrowingTheFilter()
    {
        TransactionFilter? usedFilter = null;
        _transactions
            .GetAsync(Arg.Do<TransactionFilter>(filter => usedFilter = filter), Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<Transaction>)[TestData.Transaction()]);
        _budgets.GetForMonthAsync(Arg.Any<MonthRange>(), Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<Budget>)[]);
        _categories.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<Category>)[TestData.Category()]);

        var service = new SummaryService(_transactions, _budgets, _categories);
        var summary = await service.GetAsync(MonthRange.Of(2025, 3), CancellationToken.None);

        Assert.NotNull(usedFilter);
        Assert.Null(usedFilter.Type);
        Assert.Null(usedFilter.CategoryId);
        Assert.Equal(new DateOnly(2025, 3, 1), usedFilter.Month.Start);
        Assert.Equal(900m, summary.TotalExpenses);
    }
}
