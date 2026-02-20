using IshTop.Application.Common.Models;
using IshTop.Domain.Enums;
using MediatR;

namespace IshTop.Application.Users.Commands.CreateUser;

public record CreateUserCommand(
    long TelegramId,
    string? Username,
    string? FirstName,
    string? LastName,
    LanguagePreference Language = LanguagePreference.Uzbek
) : IRequest<Result<Guid>>;
