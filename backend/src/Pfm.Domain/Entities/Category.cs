namespace Pfm.Domain.Entities;

/// <summary>
/// A bucket that transactions are assigned to, for example "Miete" or "Gehalt".
/// A category is either for income or for expenses, never for both.
/// </summary>
public sealed class Category
{
    /// <summary>Assigned by the persistence layer when the category is first stored.</summary>
    public string Id { get; set; } = string.Empty;

    public required string Name { get; set; }

    public required TransactionType Type { get; set; }

    /// <summary>Name of a Material Symbol, for example <c>shopping_cart</c>.</summary>
    public required string Icon { get; set; }

    /// <summary>Accent colour as an uppercase hex triplet, for example <c>#EF6C00</c>.</summary>
    public required string Color { get; set; }
}
