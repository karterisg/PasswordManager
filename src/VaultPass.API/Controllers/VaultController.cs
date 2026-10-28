using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VaultPass.Application.Commands.Vault;

namespace VaultPass.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VaultController(IMediator mediator) : ControllerBase
{
    private Guid CallerVaultId => Guid.Parse(User.FindFirstValue("vaultId")!);

    [HttpPatch("{id}/travel-mode")]
    public async Task<IActionResult> SetTravelMode(Guid id, [FromBody] SetTravelModeRequest req, CancellationToken ct)
    {
        if (id != CallerVaultId) return Forbid();
        await mediator.Send(new SetTravelModeCommand(id, req.Enabled), ct);
        return NoContent();
    }
}

public record SetTravelModeRequest(bool Enabled);
