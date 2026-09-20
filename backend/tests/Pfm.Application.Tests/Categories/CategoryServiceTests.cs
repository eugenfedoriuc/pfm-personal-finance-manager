using NSubstitute;
using Pfm.Application.Abstractions;
using Pfm.Application.Categories;
using Pfm.Domain;
using Pfm.Domain.Entities;
using Pfm.Domain.Exceptions;

namespace Pfm.Application.Tests.Categories;

public sealed class CategoryServiceTests
{
    private readonly ICategoryRepository _categories = Substitute.For<ICategoryRepository>();
    private readonly ITransactionRepository _transactions = Substitute.For<ITransactionRepository>();
    private readonly IBudgetRepository _budgets = Substitute.For<IBudgetRepository>();
    private readonly CategoryService _service;

    public CategoryServiceTests() => _service = new CategoryService(_categories, _transactions, _budgets);

    [Fact]
    public async Task GetAllAsync_MapsEveryCategory()
    {
        IReadOnlyList<Category> stored =
        [
            TestData.Category(TestData.IncomeCategoryId, "Gehalt", TransactionType.Income),
            TestData.Category(TestData.ExpenseCategoryId, "Miete")
        ];
        _categories.GetAllAsync(Arg.Any<CancellationToken>()).Returns(stored);

        var result = await _service.GetAllAsync(CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(
            new CategoryResponse(TestData.IncomeCategoryId, "Gehalt", TransactionType.Income, "payments", "#2E7D32"),
            result[0]);
        Assert.Equal("Miete", result[1].Name);
    }

    [Fact]
    public async Task GetAsync_ThrowsNotFound_WhenCategoryDoesNotExist()
    {
        _categories.GetByIdAsync("missing", Arg.Any<CancellationToken>()).Returns((Category?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetAsync("missing", CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_TrimsNameAndNormalisesColour()
    {
        Category? inserted = null;
        _categories
            .InsertAsync(Arg.Do<Category>(category => inserted = category), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var request = new CreateCategoryRequest("  Freizeit  ", TransactionType.Expense, "sports_esports", "#6a1b9a");
        var response = await _service.CreateAsync(request, CancellationToken.None);

        Assert.NotNull(inserted);
        Assert.Equal("Freizeit", inserted.Name);
        Assert.Equal("#6A1B9A", inserted.Color);
        Assert.Equal(TransactionType.Expense, inserted.Type);
        Assert.Equal("Freizeit", response.Name);
    }

    [Fact]
    public async Task UpdateAsync_ChangesNameIconAndColourButKeepsType()
    {
        _categories.GetByIdAsync(TestData.ExpenseCategoryId, Arg.Any<CancellationToken>())
            .Returns(TestData.Category());
        _categories.UpdateAsync(Arg.Any<Category>(), Arg.Any<CancellationToken>()).Returns(true);

        var response = await _service.UpdateAsync(
            TestData.ExpenseCategoryId,
            new UpdateCategoryRequest("Wohnen", "home", "#1565c0"),
            CancellationToken.None);

        Assert.Equal("Wohnen", response.Name);
        Assert.Equal("home", response.Icon);
        Assert.Equal("#1565C0", response.Color);
        Assert.Equal(TransactionType.Expense, response.Type);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsNotFound_WhenTheCategoryDisappearsBeforeTheWrite()
    {
        _categories.GetByIdAsync(TestData.ExpenseCategoryId, Arg.Any<CancellationToken>())
            .Returns(TestData.Category());
        _categories.UpdateAsync(Arg.Any<Category>(), Arg.Any<CancellationToken>()).Returns(false);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateAsync(
            TestData.ExpenseCategoryId,
            new UpdateCategoryRequest("Wohnen", "home", "#1565C0"),
            CancellationToken.None));
    }

    [Fact]
    public async Task DeleteAsync_ThrowsNotFound_WhenCategoryDoesNotExist()
    {
        _categories.GetByIdAsync("missing", Arg.Any<CancellationToken>()).Returns((Category?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync("missing", CancellationToken.None));
    }

    [Fact]
    public async Task DeleteAsync_ThrowsConflict_WhenTransactionsAreStillBookedOnTheCategory()
    {
        _categories.GetByIdAsync(TestData.ExpenseCategoryId, Arg.Any<CancellationToken>())
            .Returns(TestData.Category());
        _transactions.ExistsForCategoryAsync(TestData.ExpenseCategoryId, Arg.Any<CancellationToken>()).Returns(true);

        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => _service.DeleteAsync(TestData.ExpenseCategoryId, CancellationToken.None));

        Assert.Contains("Miete", exception.Message);
        await _categories.DidNotReceive().DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _budgets.DidNotReceive().DeleteForCategoryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_AlsoRemovesTheBudgetsOfTheCategory()
    {
        _categories.GetByIdAsync(TestData.ExpenseCategoryId, Arg.Any<CancellationToken>())
            .Returns(TestData.Category());
        _transactions.ExistsForCategoryAsync(TestData.ExpenseCategoryId, Arg.Any<CancellationToken>()).Returns(false);
        _categories.DeleteAsync(TestData.ExpenseCategoryId, Arg.Any<CancellationToken>()).Returns(true);

        await _service.DeleteAsync(TestData.ExpenseCategoryId, CancellationToken.None);

        await _budgets.Received(1).DeleteForCategoryAsync(TestData.ExpenseCategoryId, Arg.Any<CancellationToken>());
    }
}
