namespace IshTop.Domain.Interfaces.Repositories;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IJobRepository Jobs { get; }
    IChannelRepository Channels { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
