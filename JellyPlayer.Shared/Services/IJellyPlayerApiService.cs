using JellyPlayer.Shared.Models;

namespace JellyPlayer.Shared.Services;

public interface IJellyPlayerApiService
{
    public bool SetServer(string serverUrl);
    public void SetCollectionId(Guid collectionId);
    public Guid? GetCollectionId();
    public Task<bool> CheckServerAsync(string serverUrl);
    public Task<bool> LoginAsync(string username, string password);
    public Task StartPlaybackAsync(Guid trackId);
    public Task PausePlaybackAsync(Guid trackId);
    public Task<string?> GetTrackLyricsAsync(Guid trackId);
    public Task<List<Models.Collection>> GetCollectionsAsync();
    public Task<List<Models.Album>> GetArtistsAndAlbumsAsync(int? startIndex = null,
        int? count = null);
    public Task<Models.Album> GetAlbumAsync(Guid albumId);
    public Task<List<Models.Search>> SearchAlbum(string value);
    public Task<List<Models.Search>> SearchArtistAlbums(string value);
    public Task<List<Models.Search>> SearchTrack(string value);
    public Task<List<Models.Track>> GetTracksAsync(Guid albumId);
    public Task<Models.Track> GetTrackAsync(Guid trackId);
    public Task<byte[]?> GetAlbumArtAsync(Guid albumId);
    public Task<Stream?> GetAudioStreamAsync(Guid trackId);
    public string? GetAudioStreamUrl(Guid trackId);
    public Task StopPlaybackAsync(Guid trackId);
    public Task ResumePlaybackAsync(Guid trackId);
    public Task<List<Playlist>> GetPlaylistsAsync(Guid collectionId);
}