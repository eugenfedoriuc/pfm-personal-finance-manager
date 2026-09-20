namespace Pfm.Domain.Entities;

/// <summary>
/// A single booking: money received or money spent on a given day, assigned to a category.
/// </summary>
public sealed class Transaction
{
    /// <summary>Assigned by the persistence layer when the transaction is first stored.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Always equal to the type of the referenced category.</summary>
    public required TransactionType Type { get; set; }

    /// <summary>Always positive; the direction is expressed by <see cref="Type"/>.</summary>
    public required decimal Amount { get; set; }

    /// <summary>The calendar day of the booking, without a time or a time zone.</summary>
    public required DateOnly Date { get; set; }

    public required string CategoryId { get; set; }

    public string? Description { get; set; }
}
