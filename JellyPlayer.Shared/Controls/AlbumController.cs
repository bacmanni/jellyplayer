using JellyPlayer.Shared.Enums;
using JellyPlayer.Shared.Events;
using JellyPlayer.Shared.Models;
using JellyPlayer.Shared.Services;

namespace JellyPlayer.Shared.Controls;

public sealed class AlbumController : IDisposable
{
    private readonly IJellyPlayerApiService _jellyPlayerApiService;
    private readonly IConfigurationService _configurationService;
    private readonly IPlayerService _playerService;
    private readonly IFileService _fileService;
    
    public IPlayerService GetPlayerService() => _playerService;
    public IFileService GetFileService() => _fileService;

    public Album? Album { get; private set; }
    public List<Track> Tracks { get; private set; } = [];
    public Track? SelectedTrack { get; private set; }
    public byte[]? Artwork { get; private set; }
    public event EventHandler<AlbumStateArgs> OnAlbumChanged;
    
    public AlbumController(IJellyPlayerApiService jellyPlayerApiService, IConfigurationService configurationService, IPlayerService playerService, IFileService fileService)
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
            AlbumChanged(new AlbumStateArgs() { UpdateTrackState = true, SelectedTrackId = e.SelectedTrack.Id });
        }
    }

    /// <summary>
    /// Set track as selected
    /// </summary>
    /// <param name="trackId"></param>
    public void SelectTrack(Guid trackId)
    {
        SelectedTrack = Tracks.FirstOrDefault(t => t.Id == trackId);
    }

    public void Dispose()
    {
        _playerService.OnPlayerStateChanged -= PlayerServiceOnPlayerStateChanged;
    }

    private void AlbumChanged(AlbumStateArgs args)
    {
        OnAlbumChanged?.Invoke(this, args);
    }
    
    /// <summary>
    /// Open album
    /// </summary>
    /// <param name="albumId"></param>
    public async Task Open(Guid albumId, Guid? selectedTrackId = null)
    {
        AlbumChanged(new AlbumStateArgs());
        Album = await _jellyPlayerApiService.GetAlbumAsync(albumId);
        Tracks = await _jellyPlayerApiService.GetTracksAsync(albumId);
        AlbumChanged(new AlbumStateArgs() { UpdateAlbum = true, UpdateTracks = true, SelectedTrackId = selectedTrackId });

        if (Album.HasArtwork)
        {
            Artwork = await _fileService.GetFileAsync(FileType.AlbumArt, albumId);
            AlbumChanged(new AlbumStateArgs() { UpdateArtwork = true});    
        }
    }

    /// <summary>
    /// Play track. If already playing, then pause track
    /// </summary>
    /// <param name="trackId"></param>
    public async Task PlayOrPauseTrack(Guid trackId)
    {
        if (_playerService.IsPlaying() && _playerService.IsPlayingTrack(trackId))
        {
            _playerService.PauseTrack();
        }
        else
        {
            await _playerService.StartTrackAsync(trackId);
        }
    }

    /// <summary>
    /// Add track to play queue
    /// </summary>
    /// <param name="getTrackId"></param>
    public void AddTrackToQueue(Guid getTrackId)
    {
        var track =  Tracks.FirstOrDefault(t => t.Id == getTrackId);
        if (track != null)
            _playerService.AddTrack(track);
    }

    /// <summary>
    /// Get track position in queue
    /// </summary>
    /// <param name="trackId"></param>
    /// <returns></returns>
    public int? GetTrackPositionInQueue(Guid trackId)
    {
        var track = Tracks.FirstOrDefault(t => t.Id == trackId);
        if (track != null)
            _playerService.GetQueuePosition(trackId);
        
        return null;
    }
}