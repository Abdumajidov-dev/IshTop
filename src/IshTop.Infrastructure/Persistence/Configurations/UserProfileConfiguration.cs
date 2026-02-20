using IshTop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IshTop.Infrastructure.Persistence.Configurations;

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.TechStacks)
            .HasColumnType("text[]");

        builder.Property(p => p.SalaryMin).HasPrecision(18, 2);
        builder.Property(p => p.SalaryMax).HasPrecision(18, 2);
        builder.Property(p => p.City).HasMaxLength(100);

        builder.Property(p => p.Embedding)
            .HasColumnType("vector(1536)");
    }
}
