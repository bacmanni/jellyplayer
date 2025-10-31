using System.Collections.Concurrent;
using JellyPlayer.Shared.Enums;

namespace JellyPlayer.Shared.Services;

public class FileService : IFileService
{
    private readonly IJellyPlayerApiService _jellyPlayerApiService;
    private readonly IConfigurationService  _configurationService;
    
    // Used for caching already fetched images
    private readonly ConcurrentDictionary<string, byte[]> _artWork = [];
    private readonly SemaphoreSlim _semaphore = new(3);
    
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
        await _semaphore.WaitAsync();
        try
        {
            var key = $"{type.ToString()}-{id.ToString()}";
            var filename = GetFilename(type, id);
        
            if (type == FileType.AlbumArt && _artWork.TryGetValue(key, out var cachedArt))
            {
                return cachedArt;
            }

            if (_configurationService.Get().CacheAlbumArt)
            {
                var dir = Path.GetDirectoryName(filename);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                if (File.Exists(filename))
                {
                    var fileBytes = await File.ReadAllBytesAsync(filename);
                    _artWork.TryAdd(key, fileBytes);
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

            _artWork.TryAdd(key, primaryArt);
            return primaryArt;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
