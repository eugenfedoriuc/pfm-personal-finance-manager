using MongoDB.Bson;
using MongoDB.Driver;
using Pfm.Application.Abstractions;
using Pfm.Domain.Entities;
using Pfm.Domain.Exceptions;
using Pfm.Infrastructure.Mongo;

namespace Pfm.Infrastructure.Repositories;

internal sealed class CategoryRepository(MongoContext context) : ICategoryRepository
{
    private IMongoCollection<Category> Collection => context.Categories;

    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken) =>
        await Collection
            .Find(FilterDefinition<Category>.Empty)
            .SortBy(category => category.Name)
            .ToListAsync(cancellationToken);

    public async Task<Category?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        if (!ObjectId.TryParse(id, out _))
        {
            return null;
        }

        return await Collection.Find(category => category.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task InsertAsync(Category category, CancellationToken cancellationToken)
    {
        try
        {
            await Collection.InsertOneAsync(category, cancellationToken: cancellationToken);
        }
        catch (MongoWriteException exception) when (IsDuplicateName(exception))
        {
            throw DuplicateName(category);
        }
    }

    public async Task<bool> UpdateAsync(Category category, CancellationToken cancellationToken)
    {
        try
        {
            var result = await Collection.ReplaceOneAsync(
                stored => stored.Id == category.Id,
                category,
                cancellationToken: cancellationToken);

            return result.MatchedCount > 0;
        }
        catch (MongoWriteException exception) when (IsDuplicateName(exception))
        {
            throw DuplicateName(category);
        }
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        if (!ObjectId.TryParse(id, out _))
        {
            return false;
        }

        var result = await Collection.DeleteOneAsync(category => category.Id == id, cancellationToken);
        return result.DeletedCount > 0;
    }

    private static bool IsDuplicateName(MongoWriteException exception) =>
        exception.WriteError?.Category == ServerErrorCategory.DuplicateKey;

    private static ConflictException DuplicateName(Category category) =>
        new($"Es gibt bereits eine Kategorie namens \"{category.Name}\" für diesen Typ.");
}
