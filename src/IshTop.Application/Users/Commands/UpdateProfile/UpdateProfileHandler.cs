using IshTop.Application.Common.Models;
using IshTop.Domain.Enums;
using IshTop.Domain.Interfaces.Repositories;
using IshTop.Domain.Interfaces.Services;
using MediatR;

namespace IshTop.Application.Users.Commands.UpdateProfile;

public class UpdateProfileHandler : IRequestHandler<UpdateProfileCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJobMatchingService _matchingService;

    public UpdateProfileHandler(IUnitOfWork unitOfWork, IJobMatchingService matchingService)
    {
        _unitOfWork = unitOfWork;
        _matchingService = matchingService;
    }

    public async Task<Result> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetWithProfileByTelegramIdAsync(request.TelegramId, cancellationToken);
        if (user is null)
            return Result.Failure("User not found");

        var profile = user.Profile ?? new Domain.Entities.UserProfile { UserId = user.Id };
        profile.TechStacks = request.TechStacks;
        profile.ExperienceLevel = request.ExperienceLevel;
        profile.SalaryMin = request.SalaryMin;
        profile.SalaryMax = request.SalaryMax;
        profile.Currency = request.Currency;
        profile.WorkType = request.WorkType;
        profile.City = request.City;
        profile.EnglishLevel = request.EnglishLevel;
        profile.IsComplete = true;

        user.Profile = profile;
        user.State = UserState.Active;
        user.OnboardingStep = OnboardingStep.Completed;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Generate embedding for matching
        await _matchingService.UpdateUserEmbeddingAsync(user.Id, cancellationToken);

        return Result.Success();
    }
}
