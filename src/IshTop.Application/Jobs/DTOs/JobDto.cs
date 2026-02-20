using IshTop.Domain.Enums;

namespace IshTop.Application.Jobs.DTOs;

public record JobDto(
    Guid Id,
    string Title,
    string Description,
    string? Company,
    List<string> TechStacks,
    ExperienceLevel? ExperienceLevel,
    decimal? SalaryMin,
    decimal? SalaryMax,
    Currency? Currency,
    WorkType? WorkType,
    string? Location,
    string? ContactInfo,
    bool IsFeatured,
    DateTime CreatedAt,
    string? ChannelUsername,
    string? ChannelTitle,
    int? SourceMessageId);
