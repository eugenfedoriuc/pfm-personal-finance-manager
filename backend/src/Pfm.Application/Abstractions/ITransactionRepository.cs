using Pfm.Application.Transactions;
using Pfm.Domain.Entities;

namespace Pfm.Application.Abstractions;

/// <summary>
/// Persistence operations for <see cref="Transaction"/>. Implemented in the infrastructure layer.
/// </summary>
public interface ITransactionRepository
{
    /// <summary>Returns the matching transactions, newest first.</summary>
    Task<IReadOnlyList<Transaction>> GetAsync(TransactionFilter filter, CancellationToken cancellationToken);

    /// <summary>Returns the transaction with the given id, or <c>null</c> if it does not exist.</summary>
    Task<Transaction?> GetByIdAsync(string id, CancellationToken cancellationToken);

    /// <summary>Stores a new transaction and assigns its id.</summary>
    Task InsertAsync(Transaction transaction, CancellationToken cancellationToken);

    /// <summary>Overwrites an existing transaction. Returns <c>false</c> if it no longer exists.</summary>
    Task<bool> UpdateAsync(Transaction transaction, CancellationToken cancellationToken);

    /// <summary>Removes a transaction. Returns <c>false</c> if it did not exist.</summary>
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken);

    /// <summary>Tells whether any transaction still references the category.</summary>
    Task<bool> ExistsForCategoryAsync(string categoryId, CancellationToken cancellationToken);
}
