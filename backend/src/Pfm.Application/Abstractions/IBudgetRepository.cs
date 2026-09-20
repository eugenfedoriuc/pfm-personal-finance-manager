using Pfm.Application.Common;
using Pfm.Domain.Entities;

namespace Pfm.Application.Abstractions;

/// <summary>
/// Persistence operations for <see cref="Budget"/>. Implemented in the infrastructure layer.
/// </summary>
public interface IBudgetRepository
{
    /// <summary>Returns every budget of the given month.</summary>
    Task<IReadOnlyList<Budget>> GetForMonthAsync(MonthRange month, CancellationToken cancellationToken);

    /// <summary>
    /// Stores the limit for the category and month of <paramref name="budget"/>, creating the
    /// budget if it does not exist yet, and returns the stored document.
    /// </summary>
    Task<Budget> UpsertAsync(Budget budget, CancellationToken cancellationToken);

    /// <summary>Removes a budget. Returns <c>false</c> if it did not exist.</summary>
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken);

    /// <summary>Removes every budget of a category, used when the category itself is deleted.</summary>
    Task DeleteForCategoryAsync(string categoryId, CancellationToken cancellationToken);
}
