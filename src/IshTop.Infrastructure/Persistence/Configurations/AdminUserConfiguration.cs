using IshTop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IshTop.Infrastructure.Persistence.Configurations;

public class AdminUserConfiguration : IEntityTypeConfiguration<AdminUser>
{
    public void Configure(EntityTypeBuilder<AdminUser> builder)
    {
        builder.HasKey(a => a.Id);
        builder.HasIndex(a => a.Email).IsUnique();
        builder.Property(a => a.Email).HasMaxLength(200).IsRequired();
        builder.Property(a => a.PasswordHash).IsRequired();
        builder.Property(a => a.FullName).HasMaxLength(300);
        builder.Property(a => a.Role).HasMaxLength(50);
    }
}
