using IshTop.Domain.Entities;
using IshTop.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace IshTop.Infrastructure.Persistence.Repositories;

public class JobRepository : Repository<Job>, IJobRepository
{
    public JobRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Job>> GetMatchingJobsAsync(Vector embedding, int limit = 20, CancellationToken ct = default)
    {
        return await DbSet
            .Include(j => j.SourceChannel)
            .Where(j => j.IsActive && !j.IsSpam && j.Embedding != null)
            .OrderBy(j => j.Embedding!.CosineDistance(embedding))
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task<bool> IsDuplicateAsync(Vector embedding, double threshold = 0.95, CancellationToken ct = default)
    {
        var closest = await DbSet
            .Where(j => j.Embedding != null)
            .OrderBy(j => j.Embedding!.CosineDistance(embedding))
            .Select(j => new { Distance = j.Embedding!.CosineDistance(embedding) })
            .FirstOrDefaultAsync(ct);

        return closest is not null && (1 - closest.Distance) >= threshold;
    }

    public async Task<IReadOnlyList<Job>> GetActiveJobsAsync(int page, int pageSize, CancellationToken ct = default)
    {
        return await DbSet
            .Where(j => j.IsActive && !j.IsSpam)
            .OrderByDescending(j => j.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<Job?> GetBySourceMessageAsync(long channelId, int messageId, CancellationToken ct = default)
    {
        return await DbSet
            .Include(j => j.SourceChannel)
            .FirstOrDefaultAsync(j =>
                j.SourceChannel != null &&
                j.SourceChannel.TelegramId == channelId &&
                j.SourceMessageId == messageId, ct);
    }

    public async Task<(IReadOnlyList<Job> Items, int TotalCount)> GetByDateRangeAsync(
        DateTime from, DateTime to, int page, int pageSize, CancellationToken ct = default)
    {
        var query = DbSet
            .Include(j => j.SourceChannel)
            .Where(j => j.IsActive && !j.IsSpam && j.CreatedAt >= from && j.CreatedAt <= to)
            .OrderByDescending(j => j.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<(IReadOnlyList<Job> Items, int TotalCount)> GetSavedByUserAsync(
        Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = DbSet
            .Include(j => j.SourceChannel)
            .Where(j => j.SavedJobs.Any(sj => sj.UserId == userId))
            .OrderByDescending(j => j.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<IReadOnlyList<Job>> SearchBySkillsAsync(IReadOnlyList<string> skills, int limit = 50, CancellationToken ct = default)
    {
        if (skills.Count == 0) return [];

        var lowerSkills = skills.Select(s => s.ToLowerInvariant()).ToList();

        // Each skill is queried separately (OR logic) then merged with deduplication
        var allResults = new Dictionary<Guid, Job>();

        foreach (var skill in lowerSkills)
        {
            var s = skill;
            var matched = await DbSet
                .Include(j => j.SourceChannel)
                .Where(j => j.IsActive && !j.IsSpam)
                .Where(j =>
                    j.TechStacks.Any(t => EF.Functions.ILike(t, $"%{s}%")) ||
                    EF.Functions.ILike(j.Title, $"%{s}%") ||
                    EF.Functions.ILike(j.Description, $"%{s}%"))
                .OrderByDescending(j => j.CreatedAt)
                .Take(limit)
                .ToListAsync(ct);

            foreach (var job in matched)
                allResults.TryAdd(job.Id, job);
        }

        return allResults.Values
            .OrderByDescending(j => j.CreatedAt)
            .Take(limit)
            .ToList();
    }
}
