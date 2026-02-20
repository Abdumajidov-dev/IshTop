using IshTop.Domain.Interfaces.Repositories;

namespace IshTop.Infrastructure.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IUserRepository? _users;
    private IJobRepository? _jobs;
    private IChannelRepository? _channels;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IUserRepository Users => _users ??= new UserRepository(_context);
    public IJobRepository Jobs => _jobs ??= new JobRepository(_context);
    public IChannelRepository Channels => _channels ??= new ChannelRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
