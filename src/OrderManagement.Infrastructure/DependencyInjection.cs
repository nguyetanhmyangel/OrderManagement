using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Application.Abstractions.DomainEvents;
using OrderManagement.Application.Abstractions.Persistence;
using OrderManagement.Domain.Repositories;
using OrderManagement.Infrastructure.Database;
using OrderManagement.Infrastructure.Events;
using OrderManagement.Infrastructure.Outbox;
using OrderManagement.Infrastructure.Persistence;

namespace OrderManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connStr = configuration.GetConnectionString("Database")!;

        services.AddDbContext<ApplicationDbContext>(
            (sp, options) =>
            {
                options.UseNpgsql(connStr);
                options.AddInterceptors(
                    sp.GetRequiredService<DomainEventInterceptor>());
            });

        services.AddScoped(typeof(IRepository<,>), typeof(EfRepository<,>));
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        // Dapper (Read)
        services.AddSingleton<ISqlConnectionFactory>(_ => new SqlConnectionFactory(connStr));
       // services.AddScoped<IProductReadService, ProductReadService>();

        // Events + Outbox
        services.AddSingleton<IEventTypeRegistry, EventTypeRegistry>();
        services.AddScoped<IDomainEventsDispatcher, DomainEventsDispatcher>();
        services.Configure<OutboxOptions>(configuration.GetSection("Outbox"));
        services.AddHostedService<OutboxProcessor>();

        // Cache
        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(30),
                LocalCacheExpiration = TimeSpan.FromMinutes(5)
            };
        });

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        });

        return services;
    }
}
