using MediatR;
using VaultPass.Application.DTOs;
using VaultPass.Application.Interfaces;

namespace VaultPass.Application.Queries.Templates;

public record GetTemplatesQuery(Guid VaultId) : IRequest<List<ItemTemplateDto>>;

public class GetTemplatesQueryHandler(IItemTemplateRepository repo)
    : IRequestHandler<GetTemplatesQuery, List<ItemTemplateDto>>
{
    public async Task<List<ItemTemplateDto>> Handle(GetTemplatesQuery request, CancellationToken ct)
    {
        var templates = await repo.GetAllAsync(request.VaultId, ct);
        return templates.Select(t => new ItemTemplateDto(
            t.Id,
            t.Name,
            t.Icon,
            t.IsBuiltIn,
            t.Fields.OrderBy(f => f.Order).Select(f => new ItemTemplateFieldDto(
                f.Id,
                f.Name,
                f.FieldType.ToString(),
                f.IsEncrypted,
                f.IsRequired,
                f.Order
            )).ToList()
        )).ToList();
    }
}
