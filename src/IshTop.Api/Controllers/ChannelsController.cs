using IshTop.Domain.Entities;
using IshTop.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IshTop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChannelsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public ChannelsController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var channels = await _unitOfWork.Channels.GetAllAsync(ct);
        return Ok(channels.Select(c => new
        {
            c.Id, c.TelegramId, c.Title, c.Username,
            c.IsActive, c.JobCount, c.LastParsedAt, c.CreatedAt
        }));
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddChannelRequest request, CancellationToken ct)
    {
        var existing = await _unitOfWork.Channels.GetByTelegramIdAsync(request.TelegramId, ct);
        if (existing is not null)
            return Conflict("Channel already exists");

        var channel = new Channel
        {
            TelegramId = request.TelegramId,
            Title = request.Title,
            Username = request.Username,
            IsActive = true
        };

        await _unitOfWork.Channels.AddAsync(channel, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Created($"/api/channels/{channel.Id}", channel);
    }

    [HttpPut("{id:guid}/toggle")]
    public async Task<IActionResult> Toggle(Guid id, CancellationToken ct)
    {
        var channel = await _unitOfWork.Channels.GetByIdAsync(id, ct);
        if (channel is null) return NotFound();

        channel.IsActive = !channel.IsActive;
        await _unitOfWork.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var channel = await _unitOfWork.Channels.GetByIdAsync(id, ct);
        if (channel is null) return NotFound();

        await _unitOfWork.Channels.DeleteAsync(channel, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return NoContent();
    }

    public record AddChannelRequest(long TelegramId, string Title, string? Username);
}
