using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pfm.Application.Abstractions;
using Pfm.Infrastructure.Mongo;
using Pfm.Infrastructure.Repositories;

namespace Pfm.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Registers the MongoDB connection, the repositories and the startup initializer.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        MongoMappings.Register();

        services.Configure<MongoOptions>(configuration.GetSection(MongoOptions.SectionName));
        services.AddSingleton<MongoContext>();

        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IBudgetRepository, BudgetRepository>();

        services.AddHostedService<MongoInitializer>();

        return services;
    }
}
