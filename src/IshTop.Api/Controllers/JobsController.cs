using IshTop.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IshTop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class JobsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public JobsController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var jobs = await _unitOfWork.Jobs.GetActiveJobsAsync(page, pageSize, ct);
        var total = await _unitOfWork.Jobs.CountAsync(ct: ct);

        var items = jobs.Select(j => new
        {
            j.Id, j.Title, j.Company, j.TechStacks, j.ExperienceLevel,
            j.SalaryMin, j.SalaryMax, j.Currency, j.WorkType, j.Location,
            j.IsActive, j.IsSpam, j.IsFeatured, j.CreatedAt
        });

        return Ok(new { items, total, page, pageSize });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var job = await _unitOfWork.Jobs.GetByIdAsync(id, ct);
        if (job is null) return NotFound();
        return Ok(job);
    }

    [HttpPut("{id:guid}/moderate")]
    public async Task<IActionResult> Moderate(Guid id, [FromBody] ModerateRequest request, CancellationToken ct)
    {
        var job = await _unitOfWork.Jobs.GetByIdAsync(id, ct);
        if (job is null) return NotFound();

        job.IsActive = request.IsActive;
        job.IsSpam = request.IsSpam;
        job.IsFeatured = request.IsFeatured;

        await _unitOfWork.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var job = await _unitOfWork.Jobs.GetByIdAsync(id, ct);
        if (job is null) return NotFound();

        await _unitOfWork.Jobs.DeleteAsync(job, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return NoContent();
    }

    public record ModerateRequest(bool IsActive, bool IsSpam, bool IsFeatured);
}
