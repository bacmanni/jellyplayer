using JellyPlayer.Shared.Models;
using JellyPlayer.Shared.Services;

namespace JellyPlayer.Shared.Controls;

public class LyricsController
{
    private readonly IJellyPlayerApiService _jellyPlayerApiService;
    private readonly IPlayerService _playerService;
    
    private Guid TrackId { get; set; }
    
    /// <summary>
    /// Artist name for the track
    /// </summary>
    public string ArtistName { get; private set; }
    
    /// <summary>
    /// Currently selected track name
    /// </summary>
    public string TrackName { get; private set; }
    
    /// <summary>
    /// Currently loaded lyrics
    /// </summary>
    public string? Lyrics { get; private set; }
    
    /// <summary>
    /// Loaded track album art
    /// </summary>
    public byte[]? AlbumArt { get; private set; }
    
    /// <summary>
    /// Called when lyrics data is updated
    /// </summary>
    public event EventHandler<EventArgs> OnLyricsUpdated;
    
    
    public LyricsController(IJellyPlayerApiService jellyPlayerApiService, IPlayerService playerService)
    {
        _jellyPlayerApiService = jellyPlayerApiService;
        _playerService = playerService;
    }

    public async Task Update()
    {
        var track = _playerService.GetSelectedTrack();
        if (track == null)
            return;
        
        var album = _playerService.GetSelectedAlbum();
        if (album == null)
            return;
        
        ArtistName = album.Artist;
        TrackName  = track.Name;
        AlbumArt = _playerService.GetArtwork();
        
        Lyrics = await _jellyPlayerApiService.GetTrackLyricsAsync(track.Id); 
        OnLyricsUpdated.Invoke(this, EventArgs.Empty);
    }
}