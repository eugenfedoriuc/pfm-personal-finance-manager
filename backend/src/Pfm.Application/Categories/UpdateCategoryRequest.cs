namespace Pfm.Application.Categories;

/// <summary>
/// Payload for updating a category. The type is deliberately not updatable: it is part of the
/// contract that existing transactions and budgets rely on.
/// </summary>
/// <param name="Name">Display name, unique per type.</param>
/// <param name="Icon">Name of a Material Symbol, for example <c>shopping_cart</c>.</param>
/// <param name="Color">Accent colour as a hex triplet, for example <c>#EF6C00</c>.</param>
public sealed record UpdateCategoryRequest(string Name, string Icon, string Color);
