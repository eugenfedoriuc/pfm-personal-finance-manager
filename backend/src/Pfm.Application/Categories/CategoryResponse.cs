using Pfm.Domain;
using Pfm.Domain.Entities;

namespace Pfm.Application.Categories;

/// <summary>A category as returned by the API.</summary>
public sealed record CategoryResponse(string Id, string Name, TransactionType Type, string Icon, string Color)
{
    public static CategoryResponse From(Category category) =>
        new(category.Id, category.Name, category.Type, category.Icon, category.Color);
}
