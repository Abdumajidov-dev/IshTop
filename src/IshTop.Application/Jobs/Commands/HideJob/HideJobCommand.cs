using IshTop.Application.Common.Models;
using IshTop.Domain.Entities;
using IshTop.Domain.Interfaces.Repositories;
using MediatR;

namespace IshTop.Application.Jobs.Commands.HideJob;

public record HideJobCommand(long TelegramId, Guid JobId) : IRequest<Result>;

public class HideJobHandler : IRequestHandler<HideJobCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public HideJobHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result> Handle(HideJobCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByTelegramIdAsync(request.TelegramId, cancellationToken);
        if (user is null) return Result.Failure("User not found");

        var hidden = new HiddenJob { UserId = user.Id, JobId = request.JobId };
        user.HiddenJobs.Add(hidden);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
