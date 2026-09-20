using Pfm.Application.Categories;
using Pfm.Domain;

namespace Pfm.Application.Transactions;

/// <summary>A transaction as returned by the API, with its category embedded for display.</summary>
public sealed record TransactionResponse(
    string Id,
    TransactionType Type,
    decimal Amount,
    DateOnly Date,
    CategoryResponse Category,
    string? Description);
