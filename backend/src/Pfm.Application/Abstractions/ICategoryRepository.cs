using Pfm.Domain.Entities;

namespace Pfm.Application.Abstractions;

/// <summary>
/// Persistence operations for <see cref="Category"/>. Implemented in the infrastructure layer.
/// </summary>
public interface ICategoryRepository
{
    /// <summary>Returns every category, ordered by name.</summary>
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>Returns the category with the given id, or <c>null</c> if it does not exist.</summary>
    Task<Category?> GetByIdAsync(string id, CancellationToken cancellationToken);

    /// <summary>Stores a new category and assigns its id.</summary>
    /// <exception cref="Pfm.Domain.Exceptions.ConflictException">
    /// Another category with the same name and type already exists.
    /// </exception>
    Task InsertAsync(Category category, CancellationToken cancellationToken);

    /// <summary>Overwrites an existing category. Returns <c>false</c> if it no longer exists.</summary>
    /// <exception cref="Pfm.Domain.Exceptions.ConflictException">
    /// Another category with the same name and type already exists.
    /// </exception>
    Task<bool> UpdateAsync(Category category, CancellationToken cancellationToken);

    /// <summary>Removes a category. Returns <c>false</c> if it did not exist.</summary>
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken);
}
