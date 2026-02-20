using IshTop.Application.Common.Models;
using MediatR;

namespace IshTop.Application.Jobs.Commands.CreateJob;

public record CreateJobCommand(
    string RawText,
    long SourceChannelTelegramId,
    int SourceMessageId
) : IRequest<Result<Guid>>;
