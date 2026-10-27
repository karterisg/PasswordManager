using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VaultPass.Domain.Entities;

namespace VaultPass.Infrastructure.Persistence.Configurations;

public class VaultItemConfiguration : IEntityTypeConfiguration<VaultItem>
{
    public void Configure(EntityTypeBuilder<VaultItem> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.EncryptedData)
            .IsRequired();

        builder.Property(i => i.Type)
            .HasConversion<int>();

        builder.Property(i => i.WebsiteUrl)
            .HasMaxLength(2000);
    }
}