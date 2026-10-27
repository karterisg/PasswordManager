namespace VaultPass.Domain.Exceptions;



// This exception is thrown when a user attempts to access a vault they do not have permission to access.
// 401
public sealed class UnauthorizedVaultAccessException : DomainException
{
    public UnauthorizedVaultAccessException()
        : base("You do not have permission to access this vault.") { }
}