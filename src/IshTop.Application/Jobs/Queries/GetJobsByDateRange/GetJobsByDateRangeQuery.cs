using IshTop.Application.Common.Models;
using IshTop.Application.Jobs.DTOs;
using IshTop.Domain.Interfaces.Repositories;
using MediatR;

namespace IshTop.Application.Jobs.Queries.GetJobsByDateRange;

/// <summary>
/// Sana oralig'i bo'yicha e'lonlarni olish.
/// DateRange: "3days" | "week" | "2weeks" | "month"
/// </summary>
public record GetJobsByDateRangeQuery(
    string DateRange,
    int Page = 1,
    int PageSize = 5
) : IRequest<PaginatedList<JobDto>>;

public class GetJobsByDateRangeHandler : IRequestHandler<GetJobsByDateRangeQuery, PaginatedList<JobDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetJobsByDateRangeHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedList<JobDto>> Handle(GetJobsByDateRangeQuery request, CancellationToken cancellationToken)
    {
        var to = DateTime.UtcNow;
        var from = request.DateRange switch
        {
            "3days"  => to.AddDays(-3),
            "week"   => to.AddDays(-7),
            "2weeks" => to.AddDays(-14),
            "month"  => to.AddDays(-30),
            _        => to.AddDays(-7)
        };

        var (items, total) = await _unitOfWork.Jobs.GetByDateRangeAsync(
            from, to, request.Page, request.PageSize, cancellationToken);

        var dtos = items.Select(j => new JobDto(
            j.Id, j.Title, j.Description, j.Company, j.TechStacks,
            j.ExperienceLevel, j.SalaryMin, j.SalaryMax, j.Currency,
            j.WorkType, j.Location, j.ContactInfo, j.IsFeatured, j.CreatedAt,
            j.SourceChannel?.Username, j.SourceChannel?.Title, j.SourceMessageId
        )).ToList();

        return new PaginatedList<JobDto>(dtos, total, request.Page, request.PageSize);
    }
}
