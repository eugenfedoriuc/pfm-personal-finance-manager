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
            map.MapIdMember(category => category.Id)
                .SetSerializer(new StringSerializer(BsonType.ObjectId))
                .SetIdGenerator(StringObjectIdGenerator.Instance);
        });
    }
}
