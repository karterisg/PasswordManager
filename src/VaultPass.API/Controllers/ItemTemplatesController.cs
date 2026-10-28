using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VaultPass.Application.Commands.Templates;
using VaultPass.Application.Queries.Templates;

namespace VaultPass.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ItemTemplatesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid vaultId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetTemplatesQuery(vaultId), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTemplateCommand command, CancellationToken ct)
    {
        var id = await mediator.Send(command, ct);
        return Ok(id);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteTemplateCommand(id), ct);
        return NoContent();
    }
}
