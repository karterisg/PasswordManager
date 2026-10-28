using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VaultPass.Application.Commands.VaultItems;
using VaultPass.Application.Queries.VaultItems;

namespace VaultPass.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VaultItemsController : ControllerBase
{
    private readonly IMediator _mediator;

    public VaultItemsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private Guid CallerVaultId => Guid.Parse(User.FindFirstValue("vaultId")!);

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid vaultId,
        CancellationToken ct)
    {
        if (vaultId != CallerVaultId) return Forbid();
        var result = await _mediator.Send(new GetVaultItemsQuery(vaultId), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateVaultItemCommand command,
        CancellationToken ct)
    {
        if (command.VaultId != CallerVaultId) return Forbid();
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetAll), new { vaultId = result.VaultId }, result);
    }


    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVaultItemCommand command, CancellationToken ct)
    {
        if (id != command.Id) return BadRequest();
        await _mediator.Send(command with { CallerVaultId = CallerVaultId }, ct);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteVaultItemCommand(id, CallerVaultId), ct);
        return NoContent();
    }

    [HttpGet("{id}/history")]
    public async Task<IActionResult> GetHistory(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetVaultItemHistoryQuery(id, CallerVaultId), ct);
        return Ok(result);
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] Guid vaultId, CancellationToken ct)
    {
        if (vaultId != CallerVaultId) return Forbid();
        var result = await _mediator.Send(new ExportVaultItemsQuery(vaultId), ct);
        return Ok(result);
    }

    [HttpPost("import")]
    public async Task<IActionResult> Import([FromBody] ImportVaultItemsCommand command, CancellationToken ct)
    {
        if (command.VaultId != CallerVaultId) return Forbid();
        var count = await _mediator.Send(command, ct);
        return Ok(new { imported = count });
    }

    [HttpPatch("{id}/travel-hidden")]
    public async Task<IActionResult> SetTravelHidden(Guid id, [FromBody] SetTravelHiddenRequest req, CancellationToken ct)
    {
        await _mediator.Send(new SetItemTravelHiddenCommand(id, req.Hidden, CallerVaultId), ct);
        return NoContent();
    }
}

public record SetTravelHiddenRequest(bool Hidden);