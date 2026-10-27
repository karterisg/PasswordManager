using MediatR;
using VaultPass.Application.DTOs;
using VaultPass.Application.Interfaces;
using VaultPass.Domain.Entities;

namespace VaultPass.Application.Commands.VaultItems;

// Each command has exactly one handler — this one handles vault item creation.
public class CreateVaultItemCommandHandler : IRequestHandler<CreateVaultItemCommand, VaultItemDto>
{
    private readonly IVaultRepository _vaultRepository;
    private readonly IVaultItemRepository _vaultItemRepository;

    public CreateVaultItemCommandHandler(
        IVaultRepository vaultRepository,
        IVaultItemRepository vaultItemRepository)
    {
        _vaultRepository = vaultRepository;
        _vaultItemRepository = vaultItemRepository;
    }

    public async Task<VaultItemDto> Handle(
        CreateVaultItemCommand command,
        CancellationToken ct)
    {
        // Verify the vault exists before adding an item to it
        var vault = await _vaultRepository.GetByIdAsync(command.VaultId, ct)
            ?? throw new KeyNotFoundException($"Vault {command.VaultId} not found.");

        var item = VaultItem.Create(
            command.VaultId,
            command.Name,
            command.Type,
            command.EncryptedData,
            command.WebsiteUrl
        );

        // Keep the in-memory domain model in sync, then persist
        vault.AddItem(item);
        await _vaultItemRepository.AddAsync(item, ct);
        await _vaultItemRepository.SaveChangesAsync(ct);

        return new VaultItemDto(
            item.Id,
            item.VaultId,
            item.Name,
            item.Type,
            item.EncryptedData,
            item.WebsiteUrl,
            item.IsFavorite,
            item.LastAccessedAt,
            item.CreatedAt,
            item.UpdatedAt
        );
    }
}
