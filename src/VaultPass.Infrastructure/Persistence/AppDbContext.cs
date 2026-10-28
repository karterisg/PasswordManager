using Microsoft.EntityFrameworkCore;
using VaultPass.Domain.Entities;

namespace VaultPass.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Vault> Vaults => Set<Vault>();
    public DbSet<VaultItem> VaultItems => Set<VaultItem>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<ItemTemplate> ItemTemplates => Set<ItemTemplate>();
    public DbSet<ItemTemplateField> ItemTemplateFields => Set<ItemTemplateField>();
    public DbSet<VaultItemHistory> VaultItemHistories => Set<VaultItemHistory>();
    public DbSet<SharedItem> SharedItems => Set<SharedItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}