namespace Pfm.Domain.Entities;

/// <summary>
/// A spending limit for one expense category in one calendar month. Budgets do not recur: every
/// month that should have a limit gets its own budget.
/// </summary>
public sealed class Budget
{
    /// <summary>Assigned by the persistence layer when the budget is first stored.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>References a category of type <see cref="TransactionType.Expense"/>.</summary>
    public required string CategoryId { get; set; }

    public required int Year { get; set; }

    /// <summary>1 to 12.</summary>
    public required int Month { get; set; }

    public required decimal Limit { get; set; }
}
