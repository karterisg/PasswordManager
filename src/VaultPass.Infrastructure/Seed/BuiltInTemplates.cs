using VaultPass.Domain.Entities;
using VaultPass.Domain.Enums;

namespace VaultPass.Infrastructure.Seed;

public static class BuiltInTemplates
{
    public static List<ItemTemplate> GetAll()
    {
        var templates = new List<ItemTemplate>();

        var password = ItemTemplate.CreateBuiltIn("Password", "🔑");
        password.AddField("Username", FieldType.Email, false, false, 1);
        password.AddField("Password", FieldType.Password, true, true, 2);
        password.AddField("Website", FieldType.Url, false, false, 3);
        password.AddField("Notes", FieldType.Note, true, false, 4);
        templates.Add(password);

        var card = ItemTemplate.CreateBuiltIn("Credit Card", "💳");
        card.AddField("Cardholder", FieldType.Text, false, true, 1);
        card.AddField("Card Number", FieldType.Text, true, true, 2);
        card.AddField("Expiry Date", FieldType.Date, false, true, 3);
        card.AddField("CVV", FieldType.Password, true, true, 4);
        card.AddField("PIN", FieldType.Password, true, false, 5);
        templates.Add(card);

        var note = ItemTemplate.CreateBuiltIn("Secure Note", "📝");
        note.AddField("Title", FieldType.Text, false, true, 1);
        note.AddField("Content", FieldType.Note, true, true, 2);
        templates.Add(note);

        var ssh = ItemTemplate.CreateBuiltIn("SSH Key", "🖥️");
        ssh.AddField("Host", FieldType.Text, false, true, 1);
        ssh.AddField("Username", FieldType.Text, false, false, 2);
        ssh.AddField("Private Key", FieldType.Password, true, true, 3);
        ssh.AddField("Passphrase", FieldType.Password, true, false, 4);
        templates.Add(ssh);

        var totp = ItemTemplate.CreateBuiltIn("2FA / TOTP", "🔐");
        totp.AddField("Account", FieldType.Text, false, true, 1);
        totp.AddField("Secret Key", FieldType.Totp, true, true, 2);
        totp.AddField("Backup Codes", FieldType.Note, true, false, 3);
        templates.Add(totp);

        var flight = ItemTemplate.CreateBuiltIn("Flight Ticket", "✈️");
        flight.AddField("Booking Reference", FieldType.Text, false, true, 1);
        flight.AddField("Airline", FieldType.Text, false, true, 2);
        flight.AddField("Flight Number", FieldType.Text, false, true, 3);
        flight.AddField("Departure", FieldType.Text, false, true, 4);
        flight.AddField("Arrival", FieldType.Text, false, true, 5);
        flight.AddField("Date", FieldType.Date, false, true, 6);
        flight.AddField("Passenger", FieldType.Text, false, false, 7);
        flight.AddField("Seat", FieldType.Text, false, false, 8);
        flight.AddField("QR Code", FieldType.File, true, false, 9);
        templates.Add(flight);

        var ticket = ItemTemplate.CreateBuiltIn("Ticket / Event", "🎟️");
        ticket.AddField("Event", FieldType.Text, false, true, 1);
        ticket.AddField("Date", FieldType.Date, false, true, 2);
        ticket.AddField("Venue", FieldType.Text, false, false, 3);
        ticket.AddField("Seat", FieldType.Text, false, false, 4);
        ticket.AddField("Barcode / QR", FieldType.File, true, false, 5);
        templates.Add(ticket);

        var qr = ItemTemplate.CreateBuiltIn("QR Code", "▣");
        qr.AddField("Name", FieldType.Text, false, true, 1);
        qr.AddField("Content", FieldType.Text, true, true, 2);
        templates.Add(qr);

        var apiKey = ItemTemplate.CreateBuiltIn("API Key", "⚙️");
        apiKey.AddField("Service", FieldType.Text, false, true, 1);
        apiKey.AddField("Key", FieldType.Password, true, true, 2);
        apiKey.AddField("Expiry", FieldType.Date, false, false, 3);
        apiKey.AddField("Notes", FieldType.Note, true, false, 4);
        templates.Add(apiKey);

        var bank = ItemTemplate.CreateBuiltIn("Bank Account", "🏦");
        bank.AddField("Bank", FieldType.Text, false, true, 1);
        bank.AddField("IBAN", FieldType.Text, true, true, 2);
        bank.AddField("Account Number", FieldType.Text, true, false, 3);
        bank.AddField("Username", FieldType.Text, false, false, 4);
        bank.AddField("Password", FieldType.Password, true, false, 5);
        templates.Add(bank);

        var identity = ItemTemplate.CreateBuiltIn("Identity / Passport", "🪪");
        identity.AddField("Full Name", FieldType.Text, false, true, 1);
        identity.AddField("Document Number", FieldType.Text, true, true, 2);
        identity.AddField("Expiry Date", FieldType.Date, false, false, 3);
        identity.AddField("Nationality", FieldType.Text, false, false, 4);
        identity.AddField("Notes", FieldType.Note, true, false, 5);
        templates.Add(identity);

        return templates;
    }
}
