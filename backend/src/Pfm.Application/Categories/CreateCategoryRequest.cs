using Pfm.Domain;

namespace Pfm.Application.Categories;

/// <summary>Payload for creating a category.</summary>
/// <param name="Name">Display name, unique per type.</param>
/// <param name="Type">Whether the category is used for income or for expenses.</param>
/// <param name="Icon">Name of a Material Symbol, for example <c>shopping_cart</c>.</param>
/// <param name="Color">Accent colour as a hex triplet, for example <c>#EF6C00</c>.</param>
public sealed record CreateCategoryRequest(string Name, TransactionType Type, string Icon, string Color);
