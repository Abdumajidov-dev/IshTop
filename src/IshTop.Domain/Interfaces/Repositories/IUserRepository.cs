using IshTop.Domain.Entities;

namespace IshTop.Domain.Interfaces.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByTelegramIdAsync(long telegramId, CancellationToken ct = default);
    Task<User?> GetWithProfileAsync(Guid userId, CancellationToken ct = default);
    Task<User?> GetWithProfileByTelegramIdAsync(long telegramId, CancellationToken ct = default);
}
