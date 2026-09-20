using NSubstitute;
using Pfm.Application.Abstractions;
using Pfm.Application.Categories;
using Pfm.Domain;
using Pfm.Domain.Entities;
using Pfm.Domain.Exceptions;

namespace Pfm.Application.Tests.Categories;

public sealed class CategoryServiceTests
{
    private readonly ICategoryRepository _repository = Substitute.For<ICategoryRepository>();
    private readonly CategoryService _service;

    public CategoryServiceTests() => _service = new CategoryService(_repository);

    [Fact]
    public async Task GetAllAsync_MapsEveryCategory()
    {
        IReadOnlyList<Category> stored =
        [
            CreateCategory("1", "Gehalt", TransactionType.Income),
            CreateCategory("2", "Miete", TransactionType.Expense)
        ];
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(stored);

        var result = await _service.GetAllAsync(CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(new CategoryResponse("1", "Gehalt", TransactionType.Income, "payments", "#2E7D32"), result[0]);
        Assert.Equal("Miete", result[1].Name);
    }

    [Fact]
    public async Task GetAsync_ThrowsNotFound_WhenCategoryDoesNotExist()
    {
        _repository.GetByIdAsync("missing", Arg.Any<CancellationToken>()).Returns((Category?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetAsync("missing", CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_TrimsNameAndNormalisesColour()
    {
        Category? inserted = null;
        _repository
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
        _repository.GetByIdAsync("1", Arg.Any<CancellationToken>())
            .Returns(CreateCategory("1", "Miete", TransactionType.Expense));
        _repository.UpdateAsync(Arg.Any<Category>(), Arg.Any<CancellationToken>()).Returns(true);

        var response = await _service.UpdateAsync(
            "1",
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
        _repository.GetByIdAsync("1", Arg.Any<CancellationToken>())
            .Returns(CreateCategory("1", "Miete", TransactionType.Expense));
        _repository.UpdateAsync(Arg.Any<Category>(), Arg.Any<CancellationToken>()).Returns(false);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateAsync(
            "1",
            new UpdateCategoryRequest("Wohnen", "home", "#1565C0"),
            CancellationToken.None));
    }

    [Fact]
    public async Task DeleteAsync_ThrowsNotFound_WhenNothingWasDeleted()
    {
        _repository.DeleteAsync("missing", Arg.Any<CancellationToken>()).Returns(false);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync("missing", CancellationToken.None));
    }

    private static Category CreateCategory(string id, string name, TransactionType type) => new()
    {
        Id = id,
        Name = name,
        Type = type,
        Icon = type == TransactionType.Income ? "payments" : "home",
        Color = type == TransactionType.Income ? "#2E7D32" : "#1565C0"
    };
}
