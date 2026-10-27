using MediatR;
using Microsoft.AspNetCore.Mvc;
using VaultPass.Application.Commands.Auth;

namespace VaultPass.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterCommand command,
        CancellationToken ct)
    {
        var userId = await _mediator.Send(command, ct);
        return Ok(new { userId });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginCommand command,
        CancellationToken ct)
    {
        var token = await _mediator.Send(command, ct);
        return Ok(token);  
    }

    //[HttpPost("refresh")]
    //public async Task<IActionResult> RefreshToken(
    //    [FromBody] RefreshTokenCommand command,
    //    CancellationToken ct)
    //{
    //    var token = await _mediator.Send(command, ct);
    //    return Ok(new { token });
    //}


}