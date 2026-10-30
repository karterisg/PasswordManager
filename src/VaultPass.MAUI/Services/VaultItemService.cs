using VaultPass.MAUI.Data;
using VaultPass.MAUI.Models;

namespace VaultPass.MAUI.Services;

public class VaultItemService
{
    private readonly LocalDatabase _db;

    public VaultItemService(LocalDatabase db) => _db = db;

    public async Task<List<VaultItemModel>> GetItemsAsync(Guid vaultId)
    {
        var items = await _db.GetItemsAsync();
        var templates = BuiltInTemplates.GetAll();
        return items.Select(x =>
        {
            var tmpl = x.TemplateName is not null
                ? templates.FirstOrDefault(t => t.Name == x.TemplateName)
                : null;
            return new VaultItemModel(
                x.Id, vaultId, x.Name, x.Type, x.EncryptedData,
                x.WebsiteUrl, x.IsFavorite, x.IsHiddenInTravelMode, tmpl?.Id);
        }).ToList();
    }

    public async Task<bool> CreateItemAsync(CreateVaultItemRequest request)
    {
        var templates = BuiltInTemplates.GetAll();
        var tmplName = request.TemplateId.HasValue
            ? templates.FirstOrDefault(t => t.Id == request.TemplateId)?.Name
            : null;
        var item = new LocalVaultItem
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Type = request.Type,
            EncryptedData = request.EncryptedData,
            WebsiteUrl = request.WebsiteUrl,
            TemplateName = tmplName
        };
        await _db.SaveItemAsync(item);
        return true;
    }

    public async Task<bool> UpdateItemAsync(Guid id, string name, string encryptedData, string? websiteUrl)
    {
        var item = await _db.GetItemAsync(id);
        if (item is null) return false;

        await _db.SaveHistoryAsync(new LocalVaultItemHistory
        {
            VaultItemId = id,
            EncryptedData = item.EncryptedData
        });

        item.Name = name;
        item.EncryptedData = encryptedData;
        item.WebsiteUrl = websiteUrl;
        await _db.SaveItemAsync(item);
        return true;
    }

    public async Task<bool> DeleteItemAsync(Guid id)
    {
        await _db.DeleteItemAsync(id);
        return true;
    }

    public Task<List<ItemTemplateModel>> GetTemplatesAsync(Guid vaultId)
        => Task.FromResult(BuiltInTemplates.GetAll());

    private static class BuiltInTemplates
    {
        public static List<ItemTemplateModel> GetAll() =>
        [
            T("Password", "🔑", [
                F("Username", "email"), F("Password", "password"), F("Website", "url"), F("Notes", "note")]),
            T("Credit Card", "💳", [
                F("Cardholder", "text"), F("Card Number", "text"), F("Expiry Date", "date"), F("CVV", "password"), F("PIN", "password")]),
            T("Secure Note", "📝", [
                F("Title", "text"), F("Content", "note")]),
            T("SSH Key", "🖥️", [
                F("Host", "text"), F("Username", "text"), F("Private Key", "password"), F("Passphrase", "password")]),
            T("2FA / TOTP", "🔐", [
                F("Account", "text"), F("Secret Key", "totp"), F("Backup Codes", "note")]),
            T("Flight Ticket", "✈️", [
                F("Booking Reference", "text"), F("Airline", "text"), F("Flight Number", "text"),
                F("Departure", "text"), F("Arrival", "text"), F("Date", "date"), F("Passenger", "text"), F("Seat", "text")]),
            T("Ticket / Event", "🎟️", [
                F("Event", "text"), F("Date", "date"), F("Venue", "text"), F("Seat", "text")]),
            T("QR Code", "▣", [
                F("Name", "text"), F("Content", "text")]),
            T("API Key", "⚙️", [
                F("Service", "text"), F("Key", "password"), F("Expiry", "date"), F("Notes", "note")]),
            T("Bank Account", "🏦", [
                F("Bank", "text"), F("IBAN", "text"), F("Account Number", "text"), F("Username", "text"), F("Password", "password")]),
            T("Identity / Passport", "🪪", [
                F("Full Name", "text"), F("Document Number", "text"), F("Expiry Date", "date"), F("Nationality", "text"), F("Notes", "note")]),
        ];

        private static ItemTemplateModel T(string name, string icon, List<ItemTemplateFieldModel> fields)
            => new(Guid.NewGuid(), name, icon, true, fields);

        private static ItemTemplateFieldModel F(string name, string type, int order = 0)
            => new(Guid.NewGuid(), name, type, type is "password" or "note" or "totp", false, order);
    }

    public async Task<List<VaultItemHistoryModel>> GetHistoryAsync(Guid itemId)
    {
        var history = await _db.GetHistoryAsync(itemId);
        return history.Select(x => new VaultItemHistoryModel(x.Id, x.EncryptedData, x.CreatedAt)).ToList();
    }

    public Task<List<ExportItemModel>> ExportAsync(Guid vaultId)
        => Task.FromResult(new List<ExportItemModel>());

    public Task<string?> CreateShareAsync(Guid itemId, int expiryHours = 24, int maxUses = 1)
        => Task.FromResult<string?>(null);

    public Task<bool> SetTravelModeAsync(Guid vaultId, bool enabled)
        => Task.FromResult(true);

    public async Task<bool> SetItemTravelHiddenAsync(Guid itemId, bool hidden)
    {
        var item = await _db.GetItemAsync(itemId);
        if (item is null) return false;
        item.IsHiddenInTravelMode = hidden;
        await _db.SaveItemAsync(item);
        return true;
    }
}
