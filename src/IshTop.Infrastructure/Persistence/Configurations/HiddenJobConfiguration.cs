using IshTop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IshTop.Infrastructure.Persistence.Configurations;

public class HiddenJobConfiguration : IEntityTypeConfiguration<HiddenJob>
{
    public void Configure(EntityTypeBuilder<HiddenJob> builder)
    {
        builder.HasKey(hj => hj.Id);
        builder.HasIndex(hj => new { hj.UserId, hj.JobId }).IsUnique();

        builder.HasOne(hj => hj.User)
            .WithMany(u => u.HiddenJobs)
            .HasForeignKey(hj => hj.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
