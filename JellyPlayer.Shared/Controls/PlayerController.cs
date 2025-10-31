using JellyPlayer.Shared.Events;
using JellyPlayer.Shared.Models;
using JellyPlayer.Shared.Services;

namespace JellyPlayer.Shared.Controls;

public sealed class PlayerController : IDisposable
{
    private readonly IJellyPlayerApiService _jellyPlayerApiService;
    private readonly IConfigurationService _configurationService;
    private readonly IPlayerService _playerService;

    public event EventHandler<AlbumArgs> OnShowPlaylistClicked;
    
    public event EventHandler<AlbumArgs> OnShowShowLyricsClicked;
    
    public IPlayerService GetPlayerService() => _playerService;
    public IJellyPlayerApiService GetJellyPlayerApiService() => _jellyPlayerApiService;
    
    public Album? Album;
    public List<Track>? Tracks;
    public Track? SelectedTrack;
    public byte[]? Artwork { get; private set; }

    public PlayerController(IJellyPlayerApiService jellyPlayerApiService, IConfigurationService configurationService, IPlayerService playerService)
    {
        _jellyPlayerApiService = jellyPlayerApiService;
        _playerService = playerService;
        _configurationService = configurationService;
        _playerService.OnPlayerStateChanged += PlayerServiceOnPlayerStateChanged;
        _playerService.OnPlayerPositionChanged += PlayerServiceOnPlayerPositionChanged;
    }

    private void PlayerServiceOnPlayerPositionChanged(object? sender, PlayerPositionArgs e)
    {

    }

    private void PlayerServiceOnPlayerStateChanged(object? sender, PlayerStateArgs e)
    {
        Album = e.Album;
        Tracks = e.Tracks;
        SelectedTrack = e.SelectedTrack;
        Artwork = _playerService.GetArtwork();
    }

    public void Dispose()
    {
        _playerService.OnPlayerStateChanged -= PlayerServiceOnPlayerStateChanged;
    }
    
    /// <summary>
    /// Get lyrics for track
    /// </summary>
    /// <param name="trackId"></param>
    /// <returns></returns>
    public async Task<string?> GetLyrics(Guid trackId)
    {
         return await _jellyPlayerApiService.GetTrackLyricsAsync(trackId);
    }

    /// <summary>
    /// Open currently playing/stopped album
    /// </summary>
    public void ShowPlaylist()
    {
        if (Album == null || SelectedTrack == null)
            return;
        
        OnShowPlaylistClicked?.Invoke(this, new AlbumArgs { AlbumId = Album.Id, TrackId = SelectedTrack.Id });
    }

    /// <summary>
    /// Show lyrics for playing/stopped album
    /// </summary>
    public void ShowShowLyrics()
    {
        if (Album == null || SelectedTrack == null)
            return;
        
        OnShowShowLyricsClicked?.Invoke(this, new AlbumArgs { AlbumId = Album.Id, TrackId = SelectedTrack.Id });
    }
}