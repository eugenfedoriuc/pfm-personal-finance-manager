using MongoDB.Bson;
using MongoDB.Driver;
using Pfm.Application.Abstractions;
using Pfm.Application.Common;
using Pfm.Domain.Entities;
using Pfm.Infrastructure.Mongo;

namespace Pfm.Infrastructure.Repositories;

internal sealed class BudgetRepository(MongoContext context) : IBudgetRepository
{
    private IMongoCollection<Budget> Collection => context.Budgets;

    public async Task<IReadOnlyList<Budget>> GetForMonthAsync(MonthRange month, CancellationToken cancellationToken) =>
        await Collection
            .Find(budget => budget.Year == month.Year && budget.Month == month.Month)
            .ToListAsync(cancellationToken);

    public async Task<Budget> UpsertAsync(Budget budget, CancellationToken cancellationToken)
    {
        var identity = Builders<Budget>.Filter.Where(stored =>
            stored.CategoryId == budget.CategoryId
            && stored.Year == budget.Year
            && stored.Month == budget.Month);

        // Only the limit is written: the fields of the equality filter are applied automatically
        // when the upsert inserts, and the server generates the _id.
        return await Collection.FindOneAndUpdateAsync(
            identity,
            Builders<Budget>.Update.Set(stored => stored.Limit, budget.Limit),
            new FindOneAndUpdateOptions<Budget> { IsUpsert = true, ReturnDocument = ReturnDocument.After },
            cancellationToken);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        if (!ObjectId.TryParse(id, out _))
        {
            return false;
        }

        var result = await Collection.DeleteOneAsync(budget => budget.Id == id, cancellationToken);
        return result.DeletedCount > 0;
    }

    public async Task DeleteForCategoryAsync(string categoryId, CancellationToken cancellationToken)
    {
        if (!ObjectId.TryParse(categoryId, out _))
        {
            return;
        }

        await Collection.DeleteManyAsync(budget => budget.CategoryId == categoryId, cancellationToken);
    }
}
