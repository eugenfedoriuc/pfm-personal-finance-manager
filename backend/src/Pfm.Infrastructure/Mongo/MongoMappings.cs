using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using Pfm.Domain.Entities;

namespace Pfm.Infrastructure.Mongo;

/// <summary>
/// Maps the domain entities onto BSON. The mapping lives here so that the domain stays free of
/// MongoDB attributes and references.
/// </summary>
internal static class MongoMappings
{
    public static void Register()
    {
        // Conventions are applied while building a class map, so they have to be registered first.
        ConventionRegistry.Register(
            "pfm",
            new ConventionPack
            {
                new CamelCaseElementNameConvention(),
                new EnumRepresentationConvention(BsonType.String),
                new IgnoreExtraElementsConvention(true)
            },
            type => type.Namespace?.StartsWith("Pfm.Domain", StringComparison.Ordinal) is true);

        BsonClassMap.TryRegisterClassMap<Category>(map =>
        {
            map.AutoMap();
            MapObjectId(map, category => category.Id);
        });

        BsonClassMap.TryRegisterClassMap<Transaction>(map =>
        {
            map.AutoMap();
            MapObjectId(map, transaction => transaction.Id);
            map.MapMember(transaction => transaction.CategoryId).SetSerializer(ObjectIdAsString);
            map.MapMember(transaction => transaction.Amount).SetSerializer(Money);
            map.MapMember(transaction => transaction.Date).SetSerializer(IsoDate);
        });

        BsonClassMap.TryRegisterClassMap<Budget>(map =>
        {
            map.AutoMap();
            MapObjectId(map, budget => budget.Id);
            map.MapMember(budget => budget.CategoryId).SetSerializer(ObjectIdAsString);
            map.MapMember(budget => budget.Limit).SetSerializer(Money);
        });
    }

    /// <summary>Ids travel as strings but are stored as native <c>ObjectId</c>s.</summary>
    private static StringSerializer ObjectIdAsString { get; } = new(BsonType.ObjectId);

    /// <summary>Money is stored as <c>Decimal128</c>, never as a double.</summary>
    private static DecimalSerializer Money { get; } = new(BsonType.Decimal128);

    /// <summary>Dates are stored as <c>yyyy-MM-dd</c>: no time, no time zone, range queries still work.</summary>
    private static DateOnlySerializer IsoDate { get; } = new(BsonType.String);

    private static void MapObjectId<T>(BsonClassMap<T> map, Expression<Func<T, string>> id) =>
        map.MapIdMember(id)
            .SetSerializer(ObjectIdAsString)
            .SetIdGenerator(StringObjectIdGenerator.Instance);
}
