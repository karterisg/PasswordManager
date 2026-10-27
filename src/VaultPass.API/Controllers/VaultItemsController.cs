using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid vaultId,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetVaultItemsQuery(vaultId), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateVaultItemCommand command,
        CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetAll), new { vaultId = result.VaultId }, result);
    }


    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVaultItemCommand command, CancellationToken ct)
    {
        if (id != command.Id) return BadRequest();
        await _mediator.Send(command, ct);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteVaultItemCommand(id), ct);
        return NoContent();
    }


}