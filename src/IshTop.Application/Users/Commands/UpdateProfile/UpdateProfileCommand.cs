using IshTop.Application.Common.Models;
using IshTop.Domain.Enums;
using MediatR;

namespace IshTop.Application.Users.Commands.UpdateProfile;

public record UpdateProfileCommand(
    long TelegramId,
    List<string> TechStacks,
    ExperienceLevel ExperienceLevel,
    decimal? SalaryMin,
    decimal? SalaryMax,
    Currency Currency,
    WorkType WorkType,
    string? City,
    EnglishLevel EnglishLevel
) : IRequest<Result>;
