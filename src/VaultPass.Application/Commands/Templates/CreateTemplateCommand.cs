using MediatR;
using VaultPass.Application.Interfaces;
using VaultPass.Domain.Entities;
using VaultPass.Domain.Enums;

namespace VaultPass.Application.Commands.Templates;

public record CreateTemplateFieldRequest(string Name, FieldType FieldType, bool IsEncrypted, bool IsRequired, int Order);
public record CreateTemplateCommand(Guid VaultId, string Name, string Icon, List<CreateTemplateFieldRequest> Fields) : IRequest<Guid>;

public class CreateTemplateCommandHandler(IItemTemplateRepository repo)
    : IRequestHandler<CreateTemplateCommand, Guid>
{
    public async Task<Guid> Handle(CreateTemplateCommand request, CancellationToken ct)
    {
        var template = ItemTemplate.CreateCustom(request.VaultId, request.Name, request.Icon);
        foreach (var f in request.Fields)
            template.AddField(f.Name, f.FieldType, f.IsEncrypted, f.IsRequired, f.Order);

        await repo.AddAsync(template, ct);
        await repo.SaveChangesAsync(ct);
        return template.Id;
    }
}
