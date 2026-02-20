using IshTop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IshTop.Infrastructure.Persistence.Configurations;

public class SavedJobConfiguration : IEntityTypeConfiguration<SavedJob>
{
    public void Configure(EntityTypeBuilder<SavedJob> builder)
    {
        builder.HasKey(sj => sj.Id);
        builder.HasIndex(sj => new { sj.UserId, sj.JobId }).IsUnique();

        builder.HasOne(sj => sj.User)
            .WithMany(u => u.SavedJobs)
            .HasForeignKey(sj => sj.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sj => sj.Job)
            .WithMany(j => j.SavedJobs)
            .HasForeignKey(sj => sj.JobId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
