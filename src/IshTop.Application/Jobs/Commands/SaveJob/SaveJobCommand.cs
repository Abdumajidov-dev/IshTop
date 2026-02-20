using IshTop.Application.Common.Models;
using IshTop.Domain.Entities;
using IshTop.Domain.Interfaces.Repositories;
using MediatR;

namespace IshTop.Application.Jobs.Commands.SaveJob;

public record SaveJobCommand(long TelegramId, Guid JobId) : IRequest<Result>;

public class SaveJobHandler : IRequestHandler<SaveJobCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public SaveJobHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result> Handle(SaveJobCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByTelegramIdAsync(request.TelegramId, cancellationToken);
        if (user is null) return Result.Failure("User not found");

        var saved = new SavedJob { UserId = user.Id, JobId = request.JobId };
        user.SavedJobs.Add(saved);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
