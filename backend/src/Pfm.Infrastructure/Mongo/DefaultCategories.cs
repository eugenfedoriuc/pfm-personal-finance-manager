using Pfm.Domain;
using Pfm.Domain.Entities;

namespace Pfm.Infrastructure.Mongo;

/// <summary>
/// The categories an empty database starts with, so that the app is usable immediately.
/// </summary>
internal static class DefaultCategories
{
    /// <summary>Returns fresh instances, because inserting assigns an id to the entity.</summary>
    public static IReadOnlyList<Category> Create() =>
    [
        new() { Name = "Gehalt", Type = TransactionType.Income, Icon = "payments", Color = "#2E7D32" },
        new() { Name = "Miete", Type = TransactionType.Expense, Icon = "home", Color = "#1565C0" },
        new() { Name = "Lebensmittel", Type = TransactionType.Expense, Icon = "shopping_cart", Color = "#EF6C00" },
        new() { Name = "Freizeit", Type = TransactionType.Expense, Icon = "sports_esports", Color = "#6A1B9A" },
        new() { Name = "Transport", Type = TransactionType.Expense, Icon = "directions_car", Color = "#00838F" }
    ];
}
