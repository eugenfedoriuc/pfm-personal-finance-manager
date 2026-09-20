using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Pfm.Domain.Entities;

namespace Pfm.Infrastructure.Mongo;

/// <summary>
/// Creates the indexes and seeds the default categories before the API starts serving requests.
/// Both steps are idempotent, so restarting the container is safe.
/// </summary>
internal sealed class MongoInitializer(MongoContext context, ILogger<MongoInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await CreateIndexesAsync(cancellationToken);
        await SeedCategoriesAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task CreateIndexesAsync(CancellationToken cancellationToken)
    {
        var uniqueNamePerType = new CreateIndexModel<Category>(
            Builders<Category>.IndexKeys.Ascending(category => category.Name).Ascending(category => category.Type),
            new CreateIndexOptions
            {
                Name = "ux_categories_name_type",
                Unique = true,
                // Strength 2 ignores case and accents, so "Miete" and "miete" collide.
                Collation = new Collation("de", strength: CollationStrength.Secondary)
            });

        await context.Categories.Indexes.CreateOneAsync(uniqueNamePerType, cancellationToken: cancellationToken);
    }

    private async Task SeedCategoriesAsync(CancellationToken cancellationToken)
    {
        var existing = await context.Categories.CountDocumentsAsync(
            FilterDefinition<Category>.Empty,
            cancellationToken: cancellationToken);

        if (existing > 0)
        {
            return;
        }

        var defaults = DefaultCategories.Create();
        await context.Categories.InsertManyAsync(defaults, cancellationToken: cancellationToken);
        logger.LogInformation("Seeded {Count} default categories.", defaults.Count);
    }
}
