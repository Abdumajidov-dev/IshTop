using IshTop.Domain.Common;
using IshTop.Domain.Enums;
using Pgvector;

namespace IshTop.Domain.Entities;

public class UserProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public List<string> TechStacks { get; set; } = new();
    public ExperienceLevel ExperienceLevel { get; set; }
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public Currency Currency { get; set; } = Currency.USD;
    public WorkType WorkType { get; set; }
    public string? City { get; set; }
    public EnglishLevel EnglishLevel { get; set; }
    public bool IsComplete { get; set; }

    public Vector? Embedding { get; set; }
}
