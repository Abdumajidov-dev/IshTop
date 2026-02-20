using IshTop.Domain.Entities;
using IshTop.Domain.Interfaces.Repositories;
using IshTop.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Pgvector;

namespace IshTop.Infrastructure.Services;

public class JobMatchingService : IJobMatchingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAiService _aiService;
    private readonly ILogger<JobMatchingService> _logger;

    public JobMatchingService(IUnitOfWork unitOfWork, IAiService aiService, ILogger<JobMatchingService> logger)
    {
        _unitOfWork = unitOfWork;
        _aiService = aiService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Job>> FindMatchingJobsAsync(Guid userId, int limit = 10, CancellationToken ct = default)
    {
        var user = await _unitOfWork.Users.GetWithProfileAsync(userId, ct);
        if (user?.Profile is null)
        {
            _logger.LogWarning("User {UserId} has no profile", userId);
            return [];
        }

        var userSkills = user.Profile.TechStacks;

        // 1. Agar user TechStacks tanlagan bo'lsa — avval DB da to'g'ridan keyword search
        if (userSkills.Count > 0)
        {
            var keywordResults = await _unitOfWork.Jobs.SearchBySkillsAsync(userSkills, limit * 3, ct);

            if (keywordResults.Count >= limit)
            {
                _logger.LogInformation("Found {Count} keyword-matched jobs for user {UserId}", keywordResults.Count, userId);
                return keywordResults.Take(limit).ToList();
            }

            _logger.LogInformation("Keyword search returned {Count} results for user {UserId}", keywordResults.Count, userId);

            // Keyword natijasi kam — vector bilan to'ldiramiz (embedding mavjud bo'lsa)
            if (user.Profile.Embedding is not null)
            {
                var keywordIds = keywordResults.Select(j => j.Id).ToHashSet();
                var vectorMatches = await _unitOfWork.Jobs.GetMatchingJobsAsync(user.Profile.Embedding, limit * 5, ct);

                var combined = keywordResults.ToList();
                foreach (var job in vectorMatches)
                {
                    if (!keywordIds.Contains(job.Id))
                        combined.Add(job);
                    if (combined.Count >= limit) break;
                }

                _logger.LogInformation("Combined {Count} jobs (keyword + vector) for user {UserId}", combined.Count, userId);
                return combined.Take(limit).ToList();
            }

            return keywordResults.Take(limit).ToList();
        }

        // 2. Agar TechStacks yo'q — faqat vector matching
        if (user.Profile.Embedding is null)
        {
            _logger.LogWarning("User {UserId} has no embedding for matching", userId);
            return [];
        }

        var vectorOnly = await _unitOfWork.Jobs.GetMatchingJobsAsync(user.Profile.Embedding, limit, ct);
        _logger.LogInformation("Vector-only: found {Count} jobs for user {UserId}", vectorOnly.Count, userId);
        return vectorOnly;
    }

    public async Task UpdateUserEmbeddingAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _unitOfWork.Users.GetWithProfileAsync(userId, ct);
        if (user?.Profile is null) return;

        var text = BuildProfileText(user.Profile);
        var embedding = await _aiService.GenerateEmbeddingAsync(text, ct);
        user.Profile.Embedding = new Vector(embedding);

        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task UpdateJobEmbeddingAsync(Guid jobId, CancellationToken ct = default)
    {
        var job = await _unitOfWork.Jobs.GetByIdAsync(jobId, ct);
        if (job is null) return;

        var text = BuildJobText(job);
        var embedding = await _aiService.GenerateEmbeddingAsync(text, ct);
        job.Embedding = new Vector(embedding);

        await _unitOfWork.SaveChangesAsync(ct);
    }

    private static string BuildProfileText(UserProfile profile)
    {
        var parts = new List<string>();
        if (profile.TechStacks.Count > 0)
            parts.Add($"Skills: {string.Join(", ", profile.TechStacks)}");
        parts.Add($"Experience: {profile.ExperienceLevel}");
        parts.Add($"Work type: {profile.WorkType}");
        if (profile.City is not null)
            parts.Add($"City: {profile.City}");
        parts.Add($"English: {profile.EnglishLevel}");
        if (profile.SalaryMin.HasValue)
            parts.Add($"Salary: {profile.SalaryMin}-{profile.SalaryMax} {profile.Currency}");
        return string.Join(". ", parts);
    }

    private static string BuildJobText(Job job)
    {
        var parts = new List<string> { job.Title };
        if (job.TechStacks.Count > 0)
            parts.Add($"Stack: {string.Join(", ", job.TechStacks)}");
        if (job.ExperienceLevel.HasValue)
            parts.Add($"Level: {job.ExperienceLevel}");
        if (job.WorkType.HasValue)
            parts.Add($"Type: {job.WorkType}");
        if (job.Location is not null)
            parts.Add($"Location: {job.Location}");
        if (job.SalaryMin.HasValue)
            parts.Add($"Salary: {job.SalaryMin}-{job.SalaryMax} {job.Currency}");
        parts.Add(job.Description);
        return string.Join(". ", parts);
    }
}
