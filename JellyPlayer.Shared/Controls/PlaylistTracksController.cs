using JellyPlayer.Shared.Enums;
using JellyPlayer.Shared.Events;
using JellyPlayer.Shared.Models;
using JellyPlayer.Shared.Services;

namespace JellyPlayer.Shared.Controls;

public class PlaylistTracksController
{
    private readonly IJellyPlayerApiService _jellyPlayerApiService;
    private readonly IConfigurationService _configurationService;
    private readonly IPlayerService _playerService;
    private readonly IFileService _fileService;
    
    public IFileService GetFileService() => _fileService;
    public IPlayerService GetPlayerService() => _playerService;

    public Playlist Playlist { private set; get; }
    public readonly List<Track> Tracks = [];
    public event EventHandler<PlaylistTracksStateArgs> OnPlaylistTracksStateChanged;
    
    public PlaylistTracksController(IJellyPlayerApiService jellyPlayerApiService, IConfigurationService configurationService, IPlayerService playerService, IFileService fileService)
    {
        _jellyPlayerApiService = jellyPlayerApiService;
        _configurationService = configurationService;
        _playerService = playerService;
        _fileService = fileService;
        
        _playerService.OnPlayerStateChanged += PlayerServiceOnPlayerStateChanged;
    }

    private void PlayerServiceOnPlayerStateChanged(object? sender, PlayerStateArgs e)
    {
        if (e.State is PlayerState.Playing or PlayerState.Stopped || e.State is PlayerState.Paused)
        {
            OnPlaylistTracksStateChanged.Invoke(this, new PlaylistTracksStateArgs() {  UpdateTrackState = true, SelectedTrackId = e.SelectedTrack.Id });
        }
    }

    /// <summary>
    /// Open selected playlist tracks
    /// </summary>
    /// <param name="playlistId"></param>
    public async Task OpenPlaylist(Guid playlistId)
    {
        OnPlaylistTracksStateChanged.Invoke(this, new PlaylistTracksStateArgs() { Loading = true });
        Playlist = await _jellyPlayerApiService.GetPlaylistAsync(playlistId);
        
        Tracks.Clear();
        var tracks = await _jellyPlayerApiService.GetPlaylistTracksAsync(playlistId);
        Tracks.AddRange(tracks);
        OnPlaylistTracksStateChanged.Invoke(this, new PlaylistTracksStateArgs());
    }

    /// <summary>
    /// Start playing track from playlist. Adds playlist to queue if empty
    /// </summary>
    /// <param name="trackId"></param>
    public async Task PlayOrPauseTrack(Guid trackId)
    {
        _playerService.ClearTracks();
        _playerService.AddTracksFromPlaylist(Tracks);
        
        if (_playerService.IsPlaying() && _playerService.IsPlayingTrack(trackId))
        {
            _playerService.PauseTrack();
        }
        else
        {
            await _playerService.StartTrackAsync(trackId);
        }
    }
}