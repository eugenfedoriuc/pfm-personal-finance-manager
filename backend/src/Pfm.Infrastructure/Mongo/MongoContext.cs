using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Pfm.Domain.Entities;

namespace Pfm.Infrastructure.Mongo;

/// <summary>
/// Owns the <see cref="MongoClient"/> (registered as a singleton, because the client pools its own
/// connections) and exposes the typed collections.
/// </summary>
public sealed class MongoContext
{
    private readonly IMongoDatabase _database;

    public MongoContext(IOptions<MongoOptions> options)
    {
        var client = new MongoClient(options.Value.ConnectionString);
        _database = client.GetDatabase(options.Value.DatabaseName);
    }

    public IMongoCollection<Category> Categories => _database.GetCollection<Category>("categories");

    public IMongoCollection<Transaction> Transactions => _database.GetCollection<Transaction>("transactions");

    public IMongoCollection<Budget> Budgets => _database.GetCollection<Budget>("budgets");
}
