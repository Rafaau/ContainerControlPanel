using Blazored.SessionStorage;
using System.Text;
using System.Text.Json;

namespace ContainerControlPanel.Web.Extensions;

/// <summary>
/// Session storage service extensions.
/// </summary>
public static class SessionStorageServiceExtensions
{
    /// <summary>
    /// Saves an item encrypted.
    /// </summary>
    /// <typeparam name="T">Generic type.</typeparam>
    /// <param name="sessionStorageService">Session storage service instance.</param>
    /// <param name="key">Key.</param>
    /// <param name="item">Item.</param>
    /// <returns>Returns a task.</returns>
    public static async Task SaveItemEncryptedAsync<T>(this ISessionStorageService sessionStorageService, string key, T item)
    {
        var itemJson = JsonSerializer.Serialize(item);
        var itemJsonBytes = Encoding.UTF8.GetBytes(itemJson);
        var base64ItemJson = Convert.ToBase64String(itemJsonBytes);
        await sessionStorageService.SetItemAsync(key, base64ItemJson);
    }

    /// <summary>
    /// Reads an encrypted item.
    /// </summary>
    /// <typeparam name="T">Generic type.</typeparam>
    /// <param name="sessionStorageService">Session storage service instance.</param>
    /// <param name="key">Key.</param>
    /// <returns>Returns the item.</returns>
    public static async Task<T> ReadEncryptedItemAsync<T>(this ISessionStorageService sessionStorageService, string key)
    {
        var base64ItemJson = await sessionStorageService.GetItemAsync<string>(key);
        var itemJsonBytes = Convert.FromBase64String(base64ItemJson);
        var itemJson = Encoding.UTF8.GetString(itemJsonBytes);
        var item = JsonSerializer.Deserialize<T>(itemJson);
        return item;
    }
}