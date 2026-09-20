using Pfm.Application.Abstractions;
using Pfm.Domain.Entities;
using Pfm.Domain.Exceptions;

namespace Pfm.Application.Categories;

/// <summary>
/// Use cases for categories. Input is assumed to be validated by the API layer; what is enforced
/// here are the rules that need the stored state.
/// </summary>
public sealed class CategoryService(ICategoryRepository categories)
{
    public async Task<IReadOnlyList<CategoryResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var stored = await categories.GetAllAsync(cancellationToken);
        return [.. stored.Select(ToResponse)];
    }

    public async Task<CategoryResponse> GetAsync(string id, CancellationToken cancellationToken)
    {
        var category = await categories.GetByIdAsync(id, cancellationToken) ?? throw NotFound(id);
        return ToResponse(category);
    }

    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = new Category
        {
            Name = request.Name.Trim(),
            Type = request.Type,
            Icon = request.Icon,
            Color = request.Color.ToUpperInvariant()
        };

        await categories.InsertAsync(category, cancellationToken);
        return ToResponse(category);
    }

    public async Task<CategoryResponse> UpdateAsync(string id, UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = await categories.GetByIdAsync(id, cancellationToken) ?? throw NotFound(id);

        category.Name = request.Name.Trim();
        category.Icon = request.Icon;
        category.Color = request.Color.ToUpperInvariant();

        if (!await categories.UpdateAsync(category, cancellationToken))
        {
            throw NotFound(id);
        }

        return ToResponse(category);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken)
    {
        if (!await categories.DeleteAsync(id, cancellationToken))
        {
            throw NotFound(id);
        }
    }

    private static CategoryResponse ToResponse(Category category) =>
        new(category.Id, category.Name, category.Type, category.Icon, category.Color);

    private static NotFoundException NotFound(string id) =>
        new($"Die Kategorie \"{id}\" wurde nicht gefunden.");
}
