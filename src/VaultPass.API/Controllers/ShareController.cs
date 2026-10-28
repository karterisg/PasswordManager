using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VaultPass.Application.Commands.Sharing;
using VaultPass.Application.Queries.Sharing;

namespace VaultPass.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShareController(IMediator mediator) : ControllerBase
{
    private Guid CallerVaultId => Guid.Parse(User.FindFirstValue("vaultId")!);

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateShareRequest req, CancellationToken ct)
    {
        var token = await mediator.Send(new CreateShareCommand(req.VaultItemId, req.ExpiryHours, req.MaxUses, CallerVaultId), ct);
        return Ok(new { token });
    }

    [HttpGet("{token}")]
    public async Task<IActionResult> Get(string token, CancellationToken ct)
    {
        var result = await mediator.Send(new GetSharedItemQuery(token), ct);
        if (result is null) return NotFound();
        return Ok(result);
    }
}

public record CreateShareRequest(Guid VaultItemId, int ExpiryHours = 24, int MaxUses = 1);
