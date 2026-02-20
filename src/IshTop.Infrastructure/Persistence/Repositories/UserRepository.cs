using IshTop.Domain.Entities;
using IshTop.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IshTop.Infrastructure.Persistence.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public async Task<User?> GetByTelegramIdAsync(long telegramId, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(u => u.TelegramId == telegramId, ct);

    public async Task<User?> GetWithProfileAsync(Guid userId, CancellationToken ct = default)
        => await DbSet.Include(u => u.Profile).FirstOrDefaultAsync(u => u.Id == userId, ct);

    public async Task<User?> GetWithProfileByTelegramIdAsync(long telegramId, CancellationToken ct = default)
        => await DbSet.Include(u => u.Profile).FirstOrDefaultAsync(u => u.TelegramId == telegramId, ct);
}
