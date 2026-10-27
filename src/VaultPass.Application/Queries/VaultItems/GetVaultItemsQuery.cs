using MediatR;
using VaultPass.Application.DTOs;

namespace VaultPass.Application.Queries.VaultItems;

public record GetVaultItemsQuery(Guid VaultId)
    : IRequest<IEnumerable<VaultItemDto>>;