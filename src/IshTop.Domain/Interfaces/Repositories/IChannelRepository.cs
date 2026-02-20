using IshTop.Domain.Entities;

namespace IshTop.Domain.Interfaces.Repositories;

public interface IChannelRepository : IRepository<Channel>
{
    Task<Channel?> GetByTelegramIdAsync(long telegramId, CancellationToken ct = default);
    Task<IReadOnlyList<Channel>> GetActiveChannelsAsync(CancellationToken ct = default);
}
