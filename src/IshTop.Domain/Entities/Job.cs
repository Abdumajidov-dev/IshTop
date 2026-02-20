using IshTop.Domain.Common;
using IshTop.Domain.Enums;
using Pgvector;

namespace IshTop.Domain.Entities;

public class Job : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Company { get; set; }
    public List<string> TechStacks { get; set; } = new();
    public ExperienceLevel? ExperienceLevel { get; set; }
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public Currency? Currency { get; set; }
    public WorkType? WorkType { get; set; }
    public string? Location { get; set; }
    public string? ContactInfo { get; set; }

    public Guid? SourceChannelId { get; set; }
    public Channel? SourceChannel { get; set; }
    public int? SourceMessageId { get; set; }
    public string? RawText { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsSpam { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime? ExpiresAt { get; set; }

    public Vector? Embedding { get; set; }

    public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
    public ICollection<SavedJob> SavedJobs { get; set; } = new List<SavedJob>();
}
