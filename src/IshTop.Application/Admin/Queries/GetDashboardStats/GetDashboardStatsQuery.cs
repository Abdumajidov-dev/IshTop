using IshTop.Domain.Enums;
using IshTop.Domain.Interfaces.Repositories;
using MediatR;

namespace IshTop.Application.Admin.Queries.GetDashboardStats;

public record GetDashboardStatsQuery : IRequest<DashboardStatsDto>;

public record DashboardStatsDto(
    int TotalUsers,
    int ActiveUsers,
    int TotalJobs,
    int ActiveJobs,
    int TotalApplications,
    int TotalChannels);

public class GetDashboardStatsHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetDashboardStatsHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        var totalUsers = await _unitOfWork.Users.CountAsync(ct: cancellationToken);
        var activeUsers = await _unitOfWork.Users.CountAsync(u => u.State == UserState.Active, cancellationToken);
        var totalJobs = await _unitOfWork.Jobs.CountAsync(ct: cancellationToken);
        var activeJobs = await _unitOfWork.Jobs.CountAsync(j => j.IsActive && !j.IsSpam, cancellationToken);
        var totalChannels = await _unitOfWork.Channels.CountAsync(ct: cancellationToken);

        return new DashboardStatsDto(totalUsers, activeUsers, totalJobs, activeJobs, 0, totalChannels);
    }
}
