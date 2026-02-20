using IshTop.Application.Admin.Commands.Login;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IshTop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return Unauthorized(new { error = result.Error });

        return Ok(new { token = result.Value });
    }
}
