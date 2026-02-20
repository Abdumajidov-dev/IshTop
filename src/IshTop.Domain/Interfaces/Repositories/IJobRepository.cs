using IshTop.Domain.Entities;
using Pgvector;

namespace IshTop.Domain.Interfaces.Repositories;

public interface IJobRepository : IRepository<Job>
{
    Task<IReadOnlyList<Job>> GetMatchingJobsAsync(Vector embedding, int limit = 20, CancellationToken ct = default);
    Task<bool> IsDuplicateAsync(Vector embedding, double threshold = 0.95, CancellationToken ct = default);
    Task<IReadOnlyList<Job>> GetActiveJobsAsync(int page, int pageSize, CancellationToken ct = default);
    Task<Job?> GetBySourceMessageAsync(long channelId, int messageId, CancellationToken ct = default);

    /// <summary>Berilgan sana oralig'idagi aktiv e'lonlarni qaytaradi (pagination bilan).</summary>
    Task<(IReadOnlyList<Job> Items, int TotalCount)> GetByDateRangeAsync(
        DateTime from, DateTime to, int page, int pageSize, CancellationToken ct = default);

    /// <summary>Foydalanuvchi saqlagan e'lonlarni qaytaradi (pagination bilan).</summary>
    Task<(IReadOnlyList<Job> Items, int TotalCount)> GetSavedByUserAsync(
        Guid userId, int page, int pageSize, CancellationToken ct = default);

    /// <summary>
    /// To'g'ridan-to'g'ri DB da kalit so'zlar bo'yicha qidiradi:
    /// TechStacks massivi, Title va Description ustunlarida case-insensitive search.
    /// </summary>
    Task<IReadOnlyList<Job>> SearchBySkillsAsync(IReadOnlyList<string> skills, int limit = 50, CancellationToken ct = default);
}
