using IshTop.Domain.Interfaces.Repositories;
using IshTop.Domain.Interfaces.Services;
using IshTop.Infrastructure.AI;
using IshTop.Infrastructure.Caching;
using IshTop.Infrastructure.Persistence;
using IshTop.Infrastructure.Persistence.Repositories;
using IshTop.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace IshTop.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // PostgreSQL + pgvector
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("PostgreSQL"),
                npgsql => npgsql.UseVector()));

        // Redis
        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis") ?? "localhost:6379"));

        // Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IJobRepository, JobRepository>();
        services.AddScoped<IChannelRepository, ChannelRepository>();

        // Services
        services.AddSingleton<ICacheService, RedisCacheService>();
        services.AddScoped<IAiService, OpenAiService>();
        services.AddScoped<IJobMatchingService, JobMatchingService>();

        return services;
    }
}
