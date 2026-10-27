namespace VaultPass.Domain.Exceptions;
//from the base exception class,
//we can create specific domain exceptions for different scenarios
//such as validation errors, business rule violations, or entity not found errors.
//This will help us to handle exceptions in a more structured way and provide meaningful error messages to the users.
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
}