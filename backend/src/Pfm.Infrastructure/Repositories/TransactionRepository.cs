using MongoDB.Bson;
using MongoDB.Driver;
using Pfm.Application.Abstractions;
using Pfm.Application.Transactions;
using Pfm.Domain.Entities;
using Pfm.Infrastructure.Mongo;

namespace Pfm.Infrastructure.Repositories;

internal sealed class TransactionRepository(MongoContext context) : ITransactionRepository
{
    private IMongoCollection<Transaction> Collection => context.Transactions;

    public async Task<IReadOnlyList<Transaction>> GetAsync(
        TransactionFilter filter,
        CancellationToken cancellationToken)
    {
        var builder = Builders<Transaction>.Filter;

        // Half-open month range: the first day of the month up to, but excluding, the first day of
        // the next one (business rule 4).
        var conditions = builder.Gte(transaction => transaction.Date, filter.Month.Start)
            & builder.Lt(transaction => transaction.Date, filter.Month.EndExclusive);

        if (filter.Type is { } type)
        {
            conditions &= builder.Eq(transaction => transaction.Type, type);
        }

        if (filter.CategoryId is { } categoryId)
        {
            if (!ObjectId.TryParse(categoryId, out _))
            {
                return [];
            }

            conditions &= builder.Eq(transaction => transaction.CategoryId, categoryId);
        }

        return await Collection
            .Find(conditions)
            .SortByDescending(transaction => transaction.Date)
            .ThenByDescending(transaction => transaction.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<Transaction?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        if (!ObjectId.TryParse(id, out _))
        {
            return null;
        }

        return await Collection.Find(transaction => transaction.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public Task InsertAsync(Transaction transaction, CancellationToken cancellationToken) =>
        Collection.InsertOneAsync(transaction, cancellationToken: cancellationToken);

    public async Task<bool> UpdateAsync(Transaction transaction, CancellationToken cancellationToken)
    {
        var result = await Collection.ReplaceOneAsync(
            stored => stored.Id == transaction.Id,
            transaction,
            cancellationToken: cancellationToken);

        return result.MatchedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        if (!ObjectId.TryParse(id, out _))
        {
            return false;
        }

        var result = await Collection.DeleteOneAsync(transaction => transaction.Id == id, cancellationToken);
        return result.DeletedCount > 0;
    }

    public async Task<bool> ExistsForCategoryAsync(string categoryId, CancellationToken cancellationToken)
    {
        if (!ObjectId.TryParse(categoryId, out _))
        {
            return false;
        }

        return await Collection
            .Find(transaction => transaction.CategoryId == categoryId)
            .Limit(1)
            .AnyAsync(cancellationToken);
    }
}
