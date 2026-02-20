using IshTop.Domain.Entities;
using IshTop.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IshTop.Infrastructure.Persistence.Repositories;

public class ChannelRepository : Repository<Channel>, IChannelRepository
{
    public ChannelRepository(AppDbContext context) : base(context) { }

    public async Task<Channel?> GetByTelegramIdAsync(long telegramId, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(c => c.TelegramId == telegramId, ct);

    public async Task<IReadOnlyList<Channel>> GetActiveChannelsAsync(CancellationToken ct = default)
        => await DbSet.Where(c => c.IsActive).ToListAsync(ct);
}
