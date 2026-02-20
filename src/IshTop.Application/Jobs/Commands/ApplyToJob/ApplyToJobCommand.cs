using IshTop.Application.Common.Models;
using MediatR;

namespace IshTop.Application.Jobs.Commands.ApplyToJob;

public record ApplyToJobCommand(long TelegramId, Guid JobId) : IRequest<Result>;

public class ApplyToJobHandler : IRequestHandler<ApplyToJobCommand, Result>
{
    private readonly Domain.Interfaces.Repositories.IUnitOfWork _unitOfWork;

    public ApplyToJobHandler(Domain.Interfaces.Repositories.IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result> Handle(ApplyToJobCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByTelegramIdAsync(request.TelegramId, cancellationToken);
        if (user is null) return Result.Failure("User not found");

        var job = await _unitOfWork.Jobs.GetByIdAsync(request.JobId, cancellationToken);
        if (job is null) return Result.Failure("Job not found");

        var exists = await _unitOfWork.Jobs.ExistsAsync(
            j => j.Applications.Any(a => a.UserId == user.Id && a.JobId == request.JobId), cancellationToken);
        if (exists) return Result.Failure("Already applied");

        var application = new Domain.Entities.JobApplication
        {
            UserId = user.Id,
            JobId = request.JobId
        };

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
