using System.Collections.Concurrent;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using JellyPlayer.Shared.Enums;
using JellyPlayer.Shared.Models;

namespace JellyPlayer.Shared.Services;

public class FileService : IFileService
{
    private readonly IJellyPlayerApiService _jellyPlayerApiService;
    private readonly IConfigurationService  _configurationService;
    
    // Used for caching already fetched images
    private readonly ConcurrentDictionary<Guid, byte[]> _artWork = [];
    private readonly ConcurrentDictionary<Guid, byte[]> _playlistArtwork = [];
    
    public FileService(IJellyPlayerApiService jellyPlayerApiService, IConfigurationService configurationService)
    {
        _jellyPlayerApiService = jellyPlayerApiService;
        _configurationService = configurationService;
    }

    private string GetFilename(FileType type, Guid id)
    {
        if (type == FileType.AlbumArt)
            return $"{_configurationService.GetConfigurationDirectory()}cache/albums/{id.ToString()}.jpg";
        if (type == FileType.Playlist)
            return $"{_configurationService.GetConfigurationDirectory()}cache/playlists/{id.ToString()}.jpg";
        
        throw new NotImplementedException($"File type {type} not implemented");
    }

    /// <summary>
    /// Get file. Uses disk cache if set in configuration
    /// </summary>
    /// <param name="type"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<byte[]?> GetFileAsync(FileType type, Guid id)
    {
        var filename = GetFilename(type, id);

        if (type == FileType.AlbumArt && _artWork.ContainsKey(id))
        {
            return _artWork[id];
        }
        
        if (type == FileType.Playlist && _playlistArtwork.ContainsKey(id))
        {
            return _playlistArtwork[id];
        }

        
        if (_configurationService.Get().CacheAlbumArt)
        {
            if (!Directory.Exists(filename))
            {
                (new FileInfo(filename)).Directory.Create();
            }

            if (File.Exists(filename))
            {
                var fileBytes = await File.ReadAllBytesAsync(filename);
                
                if (type == FileType.AlbumArt)
                    _artWork.TryAdd(id, fileBytes);
                if (type == FileType.Playlist)
                    _playlistArtwork.TryAdd(id, fileBytes);
                
                return fileBytes;
            }
        }

        var primaryArt = await _jellyPlayerApiService.GetPrimaryArtAsync(id);
        if (primaryArt == null)
            return null;

        if (_configurationService.Get().CacheAlbumArt)
        {
            await File.WriteAllBytesAsync(filename, primaryArt);
        }

        if (type == FileType.AlbumArt)
            _artWork.TryAdd(id, primaryArt);
        if (type == FileType.Playlist)
            _playlistArtwork.TryAdd(id, primaryArt);

        return primaryArt;
    }

    /// <summary>
    /// Check if cache file is found
    /// </summary>
    /// <param name="key">Key used</param>
    /// <returns></returns>
    public bool HasCacheFileAsync(string key)
    {
        var filename = _configurationService.GetConfigurationDirectory() + "cache/" + key +  ".json";
        return File.Exists(filename);
    }

    /// <summary>
    /// Get data from disk cache
    /// </summary>
    /// <param name="key">Key used</param>
    /// <typeparam name="T">Type of data</typeparam>
    /// <returns></returns>
    public async Task<T?> GetCacheFileAsync<T>(string key)
    {
        var filename = _configurationService.GetConfigurationDirectory() + "cache/" + key +  ".json";
        if (File.Exists(filename))
        {
            try
            {
                var json = await File.ReadAllTextAsync(filename);
                return JsonSerializer.Deserialize<T>(json);
            }
            catch (Exception e)
            {
                return default;
            }
        }
        
        return default;
    }
    
    /// <summary>
    /// Write data to disk cache
    /// </summary>
    /// <param name="key">Key used</param>
    /// <param name="data">Data used</param>
    /// <typeparam name="T">Type of data</typeparam>
    /// <returns></returns>
    public async Task<bool> WriteCacheFileAsync<T>(string key, T data)
    {
        try
        {
            var filename = _configurationService.GetConfigurationDirectory() + "cache/" + key +  ".json";
            var json = JsonSerializer.Serialize(data,  options: new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) });

            if (HasCacheFileAsync(key))
            {
                File.Delete(filename);
            }
            
            await File.WriteAllTextAsync(filename, json);
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }
}
