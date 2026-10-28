using MediatR;
using VaultPass.Application.Interfaces;
using VaultPass.Domain.Exceptions;

namespace VaultPass.Application.Commands.Templates;

public record DeleteTemplateCommand(Guid TemplateId) : IRequest;

public class DeleteTemplateCommandHandler(IItemTemplateRepository repo)
    : IRequestHandler<DeleteTemplateCommand>
{
    public async Task Handle(DeleteTemplateCommand request, CancellationToken ct)
    {
        var template = await repo.GetByIdAsync(request.TemplateId, ct)
            ?? throw new VaultNotFoundException(request.TemplateId);
        if (template.IsBuiltIn)
            throw new InvalidOperationException("Cannot delete built-in templates.");
        await repo.DeleteAsync(request.TemplateId, ct);
        await repo.SaveChangesAsync(ct);
    }
}
