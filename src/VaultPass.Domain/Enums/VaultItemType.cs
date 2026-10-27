namespace VaultPass.Domain.Enums;

public enum VaultItemType
{
    Login = 1,  // username + password + url
    SecureNote = 2,  // secure cyptonote
    CreditCard = 3,  // card secret + card number + expiration date + cvv
    Identity = 4   // other personal information like passport, ssn, etc.
}