using IshTop.Application.Common.Models;
using IshTop.Application.Jobs.DTOs;
using IshTop.Domain.Interfaces.Repositories;
using MediatR;

namespace IshTop.Application.Jobs.Queries.GetSavedJobs;

public record GetSavedJobsQuery(long TelegramId, int Page = 1, int PageSize = 5) : IRequest<PaginatedList<JobDto>>;

public class GetSavedJobsHandler : IRequestHandler<GetSavedJobsQuery, PaginatedList<JobDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetSavedJobsHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<PaginatedList<JobDto>> Handle(GetSavedJobsQuery request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByTelegramIdAsync(request.TelegramId, cancellationToken);
        if (user is null) return new PaginatedList<JobDto>([], 0, request.Page, request.PageSize);

        var (items, total) = await _unitOfWork.Jobs.GetSavedByUserAsync(
            user.Id, request.Page, request.PageSize, cancellationToken);

        var dtos = items.Select(j => new JobDto(
            j.Id, j.Title, j.Description, j.Company, j.TechStacks,
            j.ExperienceLevel, j.SalaryMin, j.SalaryMax, j.Currency,
            j.WorkType, j.Location, j.ContactInfo, j.IsFeatured, j.CreatedAt,
            j.SourceChannel?.Username, j.SourceChannel?.Title, j.SourceMessageId
        )).ToList();

        return new PaginatedList<JobDto>(dtos, total, request.Page, request.PageSize);
    }
}
