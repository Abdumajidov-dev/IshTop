using IshTop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IshTop.Infrastructure.Persistence.Configurations;

public class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.HasKey(j => j.Id);

        builder.Property(j => j.Title).HasMaxLength(500).IsRequired();
        builder.Property(j => j.Description).IsRequired();
        builder.Property(j => j.Company).HasMaxLength(300);
        builder.Property(j => j.Location).HasMaxLength(200);
        builder.Property(j => j.ContactInfo).HasMaxLength(500);

        builder.Property(j => j.TechStacks)
            .HasColumnType("text[]");

        builder.Property(j => j.SalaryMin).HasPrecision(18, 2);
        builder.Property(j => j.SalaryMax).HasPrecision(18, 2);

        builder.Property(j => j.Embedding)
            .HasColumnType("vector(1536)");

        builder.HasIndex(j => j.IsActive);
        builder.HasIndex(j => j.CreatedAt);

        builder.HasOne(j => j.SourceChannel)
            .WithMany(c => c.Jobs)
            .HasForeignKey(j => j.SourceChannelId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
