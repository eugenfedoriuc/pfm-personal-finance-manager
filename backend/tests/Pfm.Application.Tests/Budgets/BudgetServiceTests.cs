using NSubstitute;
using Pfm.Application.Abstractions;
using Pfm.Application.Budgets;
using Pfm.Application.Common;
using Pfm.Domain;
using Pfm.Domain.Entities;
using Pfm.Domain.Exceptions;

namespace Pfm.Application.Tests.Budgets;

public sealed class BudgetServiceTests
{
    private readonly IBudgetRepository _budgets = Substitute.For<IBudgetRepository>();
    private readonly ICategoryRepository _categories = Substitute.For<ICategoryRepository>();
    private readonly BudgetService _service;

    public BudgetServiceTests()
    {
        _service = new BudgetService(_budgets, _categories);

        IReadOnlyList<Category> allCategories = [TestData.Category()];
        _categories.GetAllAsync(Arg.Any<CancellationToken>()).Returns(allCategories);
        _categories.GetByIdAsync(TestData.ExpenseCategoryId, Arg.Any<CancellationToken>())
            .Returns(TestData.Category());
        _categories.GetByIdAsync(TestData.IncomeCategoryId, Arg.Any<CancellationToken>())
            .Returns(TestData.Category(TestData.IncomeCategoryId, "Gehalt", TransactionType.Income));
    }

    [Fact]
    public async Task GetForMonthAsync_EmbedsTheCategoryOfEachBudget()
    {
        IReadOnlyList<Budget> stored = [TestData.Budget()];
        _budgets.GetForMonthAsync(Arg.Any<MonthRange>(), Arg.Any<CancellationToken>()).Returns(stored);

        var result = await _service.GetForMonthAsync(MonthRange.Of(2025, 3), CancellationToken.None);

        var budget = Assert.Single(result);
        Assert.Equal("Miete", budget.Category.Name);
        Assert.Equal(1000m, budget.Limit);
        Assert.Equal(3, budget.Month);
    }

    [Fact]
    public async Task GetForMonthAsync_DoesNotQueryCategories_WhenNoBudgetIsSet()
    {
        _budgets.GetForMonthAsync(Arg.Any<MonthRange>(), Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<Budget>)[]);

        var result = await _service.GetForMonthAsync(MonthRange.Of(2025, 3), CancellationToken.None);

        Assert.Empty(result);
        await _categories.DidNotReceive().GetAllAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpsertAsync_StoresTheLimitForTheCategoryAndMonth()
    {
        Budget? written = null;
        _budgets.UpsertAsync(Arg.Do<Budget>(budget => written = budget), Arg.Any<CancellationToken>())
            .Returns(call => TestData.Budget(limit: call.Arg<Budget>().Limit));

        var response = await _service.UpsertAsync(
            new UpsertBudgetRequest(TestData.ExpenseCategoryId, 2025, 3, 300m),
            CancellationToken.None);

        Assert.NotNull(written);
        Assert.Equal(TestData.ExpenseCategoryId, written.CategoryId);
        Assert.Equal(2025, written.Year);
        Assert.Equal(3, written.Month);
        Assert.Equal(300m, response.Limit);
        Assert.Equal("Miete", response.Category.Name);
    }

    [Fact]
    public async Task UpsertAsync_RejectsAnIncomeCategory()
    {
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _service.UpsertAsync(
            new UpsertBudgetRequest(TestData.IncomeCategoryId, 2025, 3, 300m),
            CancellationToken.None));

        Assert.Equal("categoryId", exception.PropertyName);
        Assert.Contains("Gehalt", exception.Message);
        await _budgets.DidNotReceive().UpsertAsync(Arg.Any<Budget>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpsertAsync_RejectsAnUnknownCategory()
    {
        _categories.GetByIdAsync("missing", Arg.Any<CancellationToken>()).Returns((Category?)null);

        var exception = await Assert.ThrowsAsync<ValidationException>(() => _service.UpsertAsync(
            new UpsertBudgetRequest("missing", 2025, 3, 300m),
            CancellationToken.None));

        Assert.Equal("categoryId", exception.PropertyName);
    }

    [Fact]
    public async Task UpsertAsync_RejectsAMonthOutsideOneToTwelve()
    {
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _service.UpsertAsync(
            new UpsertBudgetRequest(TestData.ExpenseCategoryId, 2025, 13, 300m),
            CancellationToken.None));

        Assert.Equal("month", exception.PropertyName);
    }

    [Fact]
    public async Task DeleteAsync_ThrowsNotFound_WhenNothingWasDeleted()
    {
        _budgets.DeleteAsync("missing", Arg.Any<CancellationToken>()).Returns(false);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync("missing", CancellationToken.None));
    }
}
