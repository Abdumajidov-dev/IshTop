using IshTop.Domain.Enums;
using IshTop.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IshTop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public UsersController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var users = await _unitOfWork.Users.GetAllAsync(ct);
        var total = users.Count;
        var paged = users.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(u => new
            {
                u.Id, u.TelegramId, u.Username, u.FirstName, u.LastName,
                u.Language, u.State, u.CreatedAt, u.NotificationsEnabled
            });

        return Ok(new { items = paged, total, page, pageSize });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var user = await _unitOfWork.Users.GetWithProfileAsync(id, ct);
        if (user is null) return NotFound();

        return Ok(new
        {
            user.Id, user.TelegramId, user.Username, user.FirstName, user.LastName,
            user.Language, user.State, user.CreatedAt, user.NotificationsEnabled,
            Profile = user.Profile is null ? null : new
            {
                user.Profile.TechStacks, user.Profile.ExperienceLevel,
                user.Profile.SalaryMin, user.Profile.SalaryMax, user.Profile.Currency,
                user.Profile.WorkType, user.Profile.City, user.Profile.EnglishLevel,
                user.Profile.IsComplete
            }
        });
    }

    [HttpPut("{id:guid}/state")]
    public async Task<IActionResult> UpdateState(Guid id, [FromBody] UserState state, CancellationToken ct)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id, ct);
        if (user is null) return NotFound();

        user.State = state;
        await _unitOfWork.SaveChangesAsync(ct);

        return NoContent();
    }
}
