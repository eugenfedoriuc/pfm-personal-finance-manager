namespace Pfm.Infrastructure.Mongo;

/// <summary>Connection settings, bound from the <c>Mongo</c> configuration section.</summary>
public sealed class MongoOptions
{
    public const string SectionName = "Mongo";

    public string ConnectionString { get; set; } = "mongodb://localhost:27017";

    public string DatabaseName { get; set; } = "pfm";
}
