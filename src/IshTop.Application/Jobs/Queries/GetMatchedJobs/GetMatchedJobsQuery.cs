using IshTop.Application.Common.Models;
using IshTop.Application.Jobs.DTOs;
using IshTop.Domain.Interfaces.Repositories;
using IshTop.Domain.Interfaces.Services;
using MediatR;

namespace IshTop.Application.Jobs.Queries.GetMatchedJobs;

public record GetMatchedJobsQuery(long TelegramId, int Page = 1, int PageSize = 5) : IRequest<PaginatedList<JobDto>>;

public class GetMatchedJobsHandler : IRequestHandler<GetMatchedJobsQuery, PaginatedList<JobDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJobMatchingService _matchingService;

    public GetMatchedJobsHandler(IUnitOfWork unitOfWork, IJobMatchingService matchingService)
    {
        _unitOfWork = unitOfWork;
        _matchingService = matchingService;
    }

    public async Task<PaginatedList<JobDto>> Handle(GetMatchedJobsQuery request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByTelegramIdAsync(request.TelegramId, cancellationToken);
        if (user is null) return new PaginatedList<JobDto>([], 0, request.Page, request.PageSize);

        // Barcha matching joblarni olamiz (max 50), keyin pagination qilamiz
        var allJobs = await _matchingService.FindMatchingJobsAsync(user.Id, 50, cancellationToken);

        var totalCount = allJobs.Count;
        var items = allJobs
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(j => new JobDto(
                j.Id, j.Title, j.Description, j.Company, j.TechStacks,
                j.ExperienceLevel, j.SalaryMin, j.SalaryMax, j.Currency,
                j.WorkType, j.Location, j.ContactInfo, j.IsFeatured, j.CreatedAt,
                j.SourceChannel?.Username, j.SourceChannel?.Title, j.SourceMessageId
            ))
            .ToList();

        return new PaginatedList<JobDto>(items, totalCount, request.Page, request.PageSize);
    }
}
