using SQLite;

namespace VaultPass.MAUI.Data;

public class LocalDatabase
{
    private SQLiteAsyncConnection? _db;

    private async Task<SQLiteAsyncConnection> GetDbAsync()
    {
        if (_db is not null) return _db;
        var path = Path.Combine(FileSystem.AppDataDirectory, "vaultpass.db3");
        _db = new SQLiteAsyncConnection(path);
        await _db.CreateTableAsync<LocalVaultItem>();
        await _db.CreateTableAsync<LocalVaultItemHistory>();
        return _db;
    }

    public async Task<List<LocalVaultItem>> GetItemsAsync()
    {
        var db = await GetDbAsync();
        return await db.Table<LocalVaultItem>().ToListAsync();
    }

    public async Task<LocalVaultItem?> GetItemAsync(Guid id)
    {
        var db = await GetDbAsync();
        return await db.Table<LocalVaultItem>().Where(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task SaveItemAsync(LocalVaultItem item)
    {
        var db = await GetDbAsync();
        var existing = await db.Table<LocalVaultItem>().Where(x => x.Id == item.Id).FirstOrDefaultAsync();
        if (existing is null)
            await db.InsertAsync(item);
        else
            await db.UpdateAsync(item);
    }

    public async Task DeleteItemAsync(Guid id)
    {
        var db = await GetDbAsync();
        await db.DeleteAsync<LocalVaultItem>(id);
    }

    public async Task SaveHistoryAsync(LocalVaultItemHistory history)
    {
        var db = await GetDbAsync();
        await db.InsertAsync(history);
    }

    public async Task<List<LocalVaultItemHistory>> GetHistoryAsync(Guid itemId)
    {
        var db = await GetDbAsync();
        return await db.Table<LocalVaultItemHistory>()
            .Where(x => x.VaultItemId == itemId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }
}
