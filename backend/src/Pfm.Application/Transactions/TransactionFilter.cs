using Pfm.Application.Common;
using Pfm.Domain;

namespace Pfm.Application.Transactions;

/// <summary>
/// Criteria for <c>GET /api/transactions</c>. The month is mandatory; type and category are
/// optional narrowing filters.
/// </summary>
public sealed record TransactionFilter(MonthRange Month, TransactionType? Type, string? CategoryId);
