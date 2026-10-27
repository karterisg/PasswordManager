namespace VaultPass.Domain.Exceptions;

// This exception is thrown when a vault with the specified id is not found in the system.
//404

public sealed class VaultNotFoundException : DomainException
{
    public VaultNotFoundException(Guid id)
        : base($"Vault with id '{id}' was not found.") { }
}