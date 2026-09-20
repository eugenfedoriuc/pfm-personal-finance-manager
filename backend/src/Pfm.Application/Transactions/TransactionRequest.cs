using Pfm.Domain;

namespace Pfm.Application.Transactions;

/// <summary>
/// Payload for creating and for updating a transaction. Both operations take the same fields, so
/// they share one contract.
/// </summary>
/// <param name="Type">Must match the type of the referenced category.</param>
/// <param name="Amount">Positive amount, at most two decimal places.</param>
/// <param name="Date">Calendar day of the booking.</param>
/// <param name="CategoryId">Id of an existing category.</param>
/// <param name="Description">Optional note, at most 200 characters.</param>
public sealed record TransactionRequest(
    TransactionType Type,
    decimal Amount,
    DateOnly Date,
    string CategoryId,
    string? Description);
