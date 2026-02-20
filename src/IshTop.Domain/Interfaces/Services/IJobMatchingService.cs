using IshTop.Domain.Entities;

namespace IshTop.Domain.Interfaces.Services;

public interface IJobMatchingService
{
    Task<IReadOnlyList<Job>> FindMatchingJobsAsync(Guid userId, int limit = 10, CancellationToken ct = default);
    Task UpdateUserEmbeddingAsync(Guid userId, CancellationToken ct = default);
    Task UpdateJobEmbeddingAsync(Guid jobId, CancellationToken ct = default);
}
