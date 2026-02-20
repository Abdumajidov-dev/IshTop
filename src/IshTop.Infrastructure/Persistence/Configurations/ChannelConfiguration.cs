using IshTop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IshTop.Infrastructure.Persistence.Configurations;

public class ChannelConfiguration : IEntityTypeConfiguration<Channel>
{
    public void Configure(EntityTypeBuilder<Channel> builder)
    {
        builder.HasKey(c => c.Id);
        builder.HasIndex(c => c.TelegramId).IsUnique();
        builder.Property(c => c.Title).HasMaxLength(300).IsRequired();
        builder.Property(c => c.Username).HasMaxLength(200);
    }
}
