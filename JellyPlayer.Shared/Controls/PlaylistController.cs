using JellyPlayer.Shared.Enums;
using JellyPlayer.Shared.Events;
using JellyPlayer.Shared.Models;
using JellyPlayer.Shared.Services;

namespace JellyPlayer.Shared.Controls;

public class PlaylistController : IDisposable
{
    private readonly IJellyPlayerApiService _jellyPlayerApiService;
    private readonly IConfigurationService _configurationService;
    private readonly IPlayerService _playerService;
    private readonly IFileService _fileService;

    public readonly List<Playlist> Playlists = [];
    
    public event EventHandler<PlaylistStateArgs> OnPlaylistStateChanged;
    
    public PlaylistController(IJellyPlayerApiService jellyPlayerApiService, IConfigurationService configurationService, IPlayerService playerService, IFileService fileService)
    {
        _jellyPlayerApiService = jellyPlayerApiService;
        _configurationService = configurationService;
        _playerService = playerService;
        _fileService = fileService;
    }

    /// <summary>
    /// Refresh playlist data
    /// </summary>
    /// <param name="reload"></param>
    public async Task RefreshPlaylist(bool reload = false)
    {
        // TODO: Add default queue
        Playlists.Clear();
        //Playlists.Add(new Playlist() { Id = Guid.NewGuid(), Name = "Queue", Type = PlaylistType.Queue });

        var playlists = await _jellyPlayerApiService.GetPlaylistsAsync(Guid.Parse("a987925015a1b40709c19d10231dfb72"));
        Playlists.AddRange(playlists);
        OnPlaylistStateChanged.Invoke(this, new PlaylistStateArgs());
    }

    public bool HasPlaylistCollection()
    {
        var configuration = _configurationService.Get();
        return !string.IsNullOrEmpty(configuration.PlaylistCollectionId);
    }

    public void Dispose()
    {

    }
}