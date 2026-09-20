using NSubstitute;
using Pfm.Application.Abstractions;
using Pfm.Application.Common;
using Pfm.Application.Transactions;
using Pfm.Domain;
using Pfm.Domain.Entities;
using Pfm.Domain.Exceptions;

namespace Pfm.Application.Tests.Transactions;

public sealed class TransactionServiceTests
{
    private readonly ITransactionRepository _transactions = Substitute.For<ITransactionRepository>();
    private readonly ICategoryRepository _categories = Substitute.For<ICategoryRepository>();
    private readonly TransactionService _service;

    public TransactionServiceTests()
    {
        _service = new TransactionService(_transactions, _categories);

        IReadOnlyList<Category> allCategories =
        [
            TestData.Category(),
            TestData.Category(TestData.IncomeCategoryId, "Gehalt", TransactionType.Income)
        ];
        _categories.GetAllAsync(Arg.Any<CancellationToken>()).Returns(allCategories);
        _categories.GetByIdAsync(TestData.ExpenseCategoryId, Arg.Any<CancellationToken>())
            .Returns(TestData.Category());
        _categories.GetByIdAsync(TestData.IncomeCategoryId, Arg.Any<CancellationToken>())
            .Returns(TestData.Category(TestData.IncomeCategoryId, "Gehalt", TransactionType.Income));
    }

    [Fact]
    public async Task GetAsync_EmbedsTheCategoryOfEachTransaction()
    {
        IReadOnlyList<Transaction> stored = [TestData.Transaction()];
        _transactions.GetAsync(Arg.Any<TransactionFilter>(), Arg.Any<CancellationToken>()).Returns(stored);

        var result = await _service.GetAsync(
            new TransactionFilter(MonthRange.Of(2025, 3), null, null),
            CancellationToken.None);

        var transaction = Assert.Single(result);
        Assert.Equal("Miete", transaction.Category.Name);
        Assert.Equal("#1565C0", transaction.Category.Color);
        Assert.Equal(900m, transaction.Amount);
        Assert.Equal(new DateOnly(2025, 3, 4), transaction.Date);
    }

    [Fact]
    public async Task GetAsync_DoesNotQueryCategories_WhenTheMonthIsEmpty()
    {
        _transactions.GetAsync(Arg.Any<TransactionFilter>(), Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<Transaction>)[]);

        var result = await _service.GetAsync(
            new TransactionFilter(MonthRange.Of(2025, 3), null, null),
            CancellationToken.None);

        Assert.Empty(result);
        await _categories.DidNotReceive().GetAllAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_StoresTheTransactionAndReturnsItWithItsCategory()
    {
        Transaction? inserted = null;
        _transactions
            .InsertAsync(Arg.Do<Transaction>(transaction => inserted = transaction), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var response = await _service.CreateAsync(
            new TransactionRequest(
                TransactionType.Expense,
                900m,
                new DateOnly(2025, 3, 4),
                TestData.ExpenseCategoryId,
                "  Märzmiete  "),
            CancellationToken.None);

        Assert.NotNull(inserted);
        Assert.Equal(900m, inserted.Amount);
        Assert.Equal("Märzmiete", inserted.Description);
        Assert.Equal("Miete", response.Category.Name);
    }

    [Fact]
    public async Task CreateAsync_TurnsABlankDescriptionIntoNull()
    {
        Transaction? inserted = null;
        _transactions
            .InsertAsync(Arg.Do<Transaction>(transaction => inserted = transaction), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        await _service.CreateAsync(
            new TransactionRequest(
                TransactionType.Expense, 900m, new DateOnly(2025, 3, 4), TestData.ExpenseCategoryId, "   "),
            CancellationToken.None);

        Assert.NotNull(inserted);
        Assert.Null(inserted.Description);
    }

    [Fact]
    public async Task CreateAsync_RejectsATypeThatDoesNotMatchTheCategory()
    {
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(
            new TransactionRequest(
                TransactionType.Income, 900m, new DateOnly(2025, 3, 4), TestData.ExpenseCategoryId, null),
            CancellationToken.None));

        Assert.Equal("categoryId", exception.PropertyName);
        Assert.Contains("Miete", exception.Message);
        await _transactions.DidNotReceive().InsertAsync(Arg.Any<Transaction>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_RejectsAnUnknownCategory()
    {
        _categories.GetByIdAsync("missing", Arg.Any<CancellationToken>()).Returns((Category?)null);

        var exception = await Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(
            new TransactionRequest(TransactionType.Expense, 900m, new DateOnly(2025, 3, 4), "missing", null),
            CancellationToken.None));

        Assert.Equal("categoryId", exception.PropertyName);
    }

    [Fact]
    public async Task UpdateAsync_MovesTheTransactionToAnotherCategory()
    {
        _transactions.GetByIdAsync("bbbbbbbbbbbbbbbbbbbbbbb1", Arg.Any<CancellationToken>())
            .Returns(TestData.Transaction());
        _transactions.UpdateAsync(Arg.Any<Transaction>(), Arg.Any<CancellationToken>()).Returns(true);

        var response = await _service.UpdateAsync(
            "bbbbbbbbbbbbbbbbbbbbbbb1",
            new TransactionRequest(
                TransactionType.Income, 2500m, new DateOnly(2025, 3, 31), TestData.IncomeCategoryId, null),
            CancellationToken.None);

        Assert.Equal(TransactionType.Income, response.Type);
        Assert.Equal(2500m, response.Amount);
        Assert.Equal("Gehalt", response.Category.Name);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsNotFound_WhenTheTransactionDoesNotExist()
    {
        _transactions.GetByIdAsync("missing", Arg.Any<CancellationToken>()).Returns((Transaction?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateAsync(
            "missing",
            new TransactionRequest(
                TransactionType.Expense, 900m, new DateOnly(2025, 3, 4), TestData.ExpenseCategoryId, null),
            CancellationToken.None));
    }

    [Fact]
    public async Task DeleteAsync_ThrowsNotFound_WhenNothingWasDeleted()
    {
        _transactions.DeleteAsync("missing", Arg.Any<CancellationToken>()).Returns(false);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync("missing", CancellationToken.None));
    }
}
