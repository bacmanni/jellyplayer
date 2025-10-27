using JellyPlayer.Shared.Enums;
using JellyPlayer.Shared.Events;
using JellyPlayer.Shared.Models;

namespace JellyPlayer.Shared.Services;

public interface IPlayerService 
{
    public event EventHandler<PlayerStateArgs> OnPlayerStateChanged;
    public event EventHandler<PlayerPositionArgs> OnPlayerPositionChanged;
    public void SelectTrack(Guid trackId);
    public bool IsSelectedTrack(Guid? trackId = null);
    public Task StartTrackAsync(Guid? trackId = null);
    public void StopTrack();
    public void PauseTrack();
    public bool IsPlaying();
    public bool IsPaused();
    public void ShuffleTracks();
    public bool HasTracks(bool countSelected = true);
    public Task NextTrack();
    public Task PreviousTrack();
    public Guid? GetSelectedTrackId();
    public bool IsPlayingTrack(Guid? trackId, Guid? albumId = null);
    byte[]? GetArtwork();
    public Track? GetSelectedTrack();
    public Album? GetSelectedAlbum();
    public List<Track> GetTracks();
    public void AddTrack(Track track);
    public int? GetQueuePosition(Guid trackId);
    public void AddTracksFromPlaylist(List<Track> tracks);
    public void ClearTracks();
    void PlayTrack(Track track);
    public PlayerState GetTrackState(Guid trackId);
}