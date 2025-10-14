using JellyPlayer.Shared.Enums;
using JellyPlayer.Shared.Models;

namespace JellyPlayer.Shared.Services;

public interface IFileService
{
    public Task<byte[]?> GetFileAsync(FileType type, Guid id);
    public bool HasCacheFileAsync(string key);
    public Task<T?> GetCacheFileAsync<T>(string key);
    public Task<bool> WriteCacheFileAsync<T>(string key, T data);
}