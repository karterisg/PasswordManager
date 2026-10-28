using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VaultPass.Application.Interfaces;
using VaultPass.Infrastructure.Persistence;
using VaultPass.Infrastructure.Repositories;
using VaultPass.Infrastructure.Services;

namespace VaultPass.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        //services.AddStackExchangeRedisCache(options =>
        //    options.Configuration = configuration.GetConnectionString("Redis"));
        services.AddDistributedMemoryCache();

        services.AddScoped<IVaultRepository, VaultRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IVaultItemRepository, VaultItemRepository>();
        services.AddScoped<IItemTemplateRepository, ItemTemplateRepository>();
        services.AddScoped<IJwtService, JwtService>();

        return services;
    }
}