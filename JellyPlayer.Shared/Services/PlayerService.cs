
using System.Collections.Concurrent;
using JellyPlayer.Shared.Enums;
using JellyPlayer.Shared.Events;
using JellyPlayer.Shared.Models;
using SoundFlow.Abstracts.Devices;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Components;
using SoundFlow.Enums;
using SoundFlow.Providers;
using SoundFlow.Structs;
using Task = System.Threading.Tasks.Task;

namespace JellyPlayer.Shared.Services;

public class PlayerService : IPlayerService, IDisposable
{
    private readonly IJellyPlayerApiService _jellyPlayerApiService;
    private readonly Configuration _configuration;

    private readonly MiniAudioEngine _engine = new();
    private readonly AudioFormat _format = AudioFormat.Dvd;
    private readonly AudioPlaybackDevice _device;
    private SoundPlayer? _player;
    
    /// <summary>
    /// Currently selected album
    /// </summary>
    private Album? _album { get; set; }
    
    /// <summary>
    /// Currently selected albums tracks
    /// </summary>
    private ConcurrentBag<Track> _tracks { get; } = [];
    
    /// <summary>
    /// Album artwork if found
    /// </summary>
    private byte[]? _artwork { get; set; }
    
    /// <summary>
    /// Currently selected track
    /// </summary>
    private Track? _selectedTrack;
    
    /// <summary>
    /// Currently started track
    /// </summary>
    private Track? _playingTrack;
    
    /// <summary>
    /// Event for all playing related changes
    /// </summary>
    public event EventHandler<PlayerStateArgs>? OnPlayerStateChanged;
    
    /// <summary>
    /// Updates currently playing track position
    /// This is called actively so use only if needed
    /// </summary>
    public event EventHandler<PlayerPositionArgs>? OnPlayerPositionChanged;
    
    public PlayerService(IJellyPlayerApiService jellyPlayerApiService)
    {
        _jellyPlayerApiService = jellyPlayerApiService;

        var defaultDevice = _engine.PlaybackDevices.FirstOrDefault(x => x.IsDefault);
        _device = _engine.InitializePlaybackDevice(defaultDevice, _format);
        _device.Start();
    }

    private async Task OpenAlbumWithoutTracks(Guid albumId)
    {
        PlayerStateChanged(new PlayerStateArgs(PlayerState.Loading));

        var album = await _jellyPlayerApiService.GetAlbumAsync(albumId);
        _album = album ?? throw new Exception($"Album with id {albumId} not found");
        _selectedTrack = null;
        
        PlayerStateChanged(new PlayerStateArgs(PlayerState.LoadedInfo, album, _tracks.ToList()));

        if (album.HasArtwork)
        {
            _artwork = await _jellyPlayerApiService.GetPrimaryArtAsync(albumId);
            PlayerStateChanged(new PlayerStateArgs(PlayerState.LoadedArtwork, album, _tracks.ToList()));
        }
    }

    private async Task OpenAlbum(Guid albumId)
    {
        PlayerStateChanged(new PlayerStateArgs(PlayerState.Loading));

        _tracks.Clear();
        var album = await _jellyPlayerApiService.GetAlbumAsync(albumId);
        _album = album ?? throw new Exception($"Album with id {albumId} not found");
        
        var tracks = await _jellyPlayerApiService.GetTracksAsync(_album.Id);

        foreach (var track in tracks)
            _tracks.Add(track);
        
        _selectedTrack = null;

        PlayerStateChanged(new PlayerStateArgs(PlayerState.LoadedInfo, album, tracks));

        if (album.HasArtwork)
        {
            _artwork = await _jellyPlayerApiService.GetPrimaryArtAsync(albumId);
            PlayerStateChanged(new PlayerStateArgs(PlayerState.LoadedArtwork, album, tracks));
        }
    }
    
    private protected virtual void PlayerStateChanged(PlayerStateArgs e)
    {
        OnPlayerStateChanged?.Invoke(this, e);
    }
    
    private async Task PlayTrack()
    {
        if (_selectedTrack == null)
            return;

        var trackId = _selectedTrack.Id;

        // Check player status
        if (_player != null)
        {
            // Still same as selected, so we keep playing
            if (trackId == _playingTrack?.Id)
            {
                _ = _jellyPlayerApiService.ResumePlaybackAsync(trackId);
                _player.Play();
                return;
            }

            StopPlaying();
        }
        
        // Start new play session
        _ = _jellyPlayerApiService.StartPlaybackAsync(trackId);
        _playingTrack = _tracks.FirstOrDefault(t => t.Id == trackId);

        // Get stream url and start playing
        var url = _jellyPlayerApiService.GetAudioStreamUrl(trackId);
        _player = new SoundPlayer(_engine, _device.Format, new NetworkDataProvider(_engine, _format, url));
        _device.MasterMixer.AddComponent(_player);
        _player.IsLooping = false;
        _player.Play();
        _player.PlaybackEnded += async (_, args) => await OnPlaybackEnded(_, args);
        // (sender, args) => { NextTrack(); };  async (_, args) => await ControllerOnOnAlbumListChanged(_, args);
    }

    private async Task OnPlaybackEnded(object? sender, EventArgs args)
    {
        _ = NextTrack();
    }

    private void StopPlaying()
    {
        if (_playingTrack == null)
            return;

        var trackId = _playingTrack.Id;
        
        if (_player != null)
        {
            _player.PlaybackEnded -= async (_, args) => await OnPlaybackEnded(_, args);
            _jellyPlayerApiService.StopPlaybackAsync(trackId);
            _player?.Stop();
            _device.MasterMixer.RemoveComponent(_player);
            _player.Dispose();
            _player = null;
            _playingTrack = null;
        }
    }

    private void PausePlaying()
    {
        if (_playingTrack == null)
            return;

        var trackId = _playingTrack.Id;
        
        if (_player != null)
        {
            _jellyPlayerApiService.PausePlaybackAsync(trackId);
            _player.Pause();
        }
    }
    
    /// <summary>
    /// Select track from album
    /// </summary>
    /// <param name="trackId">Id of the track</param>
    public void SelectTrack(Guid trackId)
    {
        var track = _tracks.FirstOrDefault(t => t.Id == trackId);
        if (track == null) return;
        
        _selectedTrack = track;
        PlayerStateChanged(new PlayerStateArgs(PlayerState.Selected, _album, _tracks.ToList(), _selectedTrack));
    }

    /// <summary>
    /// Start playing track
    /// </summary>
    /// <param name="trackId">Id of the track. If not set uses first from the album tracks</param>
    public async Task StartTrackAsync(Guid? trackId = null)
    {
        if (!trackId.HasValue)
        {
            if (_tracks.Count > 0)
                trackId = _tracks.First().Id;
        }
        
        // Can't start anything :(
        if (!trackId.HasValue)
            return;

        var track = _tracks.FirstOrDefault(t => t.Id == trackId.Value);
        
        // Null when trying to start from album details
        if (track == null)
        {
            PlayerStateChanged(new PlayerStateArgs(PlayerState.Loading));
            track = await _jellyPlayerApiService.GetTrackAsync(trackId.Value);
            await OpenAlbum(track.AlbumId);
        }
        // Invalid id when trying to start from queue
        else if (track.AlbumId != _album?.Id)
        {
            await OpenAlbumWithoutTracks(track.AlbumId);
        }
        
        if (_selectedTrack == null || _selectedTrack.Id != trackId.Value)
        {
            SelectTrack(trackId.Value);
        }
        
        await PlayTrack();
        PlayerStateChanged(new PlayerStateArgs(PlayerState.Playing, _album, _tracks.ToList(), _selectedTrack));
    }

    /// <summary>
    /// Pause playing track
    /// </summary>
    public void PauseTrack()
    {
        if (_playingTrack != null)
        {
            PausePlaying();
            PlayerStateChanged(new PlayerStateArgs(PlayerState.Paused, _album, _tracks.ToList(), _selectedTrack));
        }
    }
    
    /// <summary>
    /// Stop playing started track
    /// </summary>
    public void StopTrack()
    {
        if (_playingTrack != null)
        {
            StopPlaying();
            PlayerStateChanged(new PlayerStateArgs(PlayerState.Stopped, _album, _tracks.ToList(), _selectedTrack));
        }
    }

    /// <summary>
    /// Shuffle queue
    /// </summary>
    public void ShuffleTracks()
    {
        var tracks = _tracks.ToArray();
        Random.Shared.Shuffle(tracks);
        _tracks.Clear();
        AddTracksFromPlaylist(tracks.ToList());
    }

    /// <summary>
    /// Check if playlist contains tracks
    /// </summary>
    /// <param name="countSelected">Default for checking if there are any tracks</param>
    /// <returns></returns>
    public bool HasTracks(bool countSelected = true)
    {
        if (countSelected)
        {
            return _tracks.Any();
        }

        if (_tracks.Count == 1)
            return _tracks.First().Id != _selectedTrack?.Id;
        else
            return true;
    }
    
    /// <summary>
    /// Select next track from album tracks
    /// </summary>
    public async Task NextTrack()
    {
        if (_selectedTrack != null)
        {
            var isPlaying = IsPlayingTrack(_selectedTrack.Id);
            var nextTrack = _tracks.Reverse().SkipWhile(t => t != _selectedTrack).Skip(1).FirstOrDefault();

            if (nextTrack == null)
            {
                StopTrack();
                PlayerStateChanged(new PlayerStateArgs(PlayerState.None, _album, _tracks.ToList(), null));
                return;
            };
            
            SelectTrack(nextTrack.Id);
            PlayerStateChanged(new PlayerStateArgs(PlayerState.SkipNext, _album, _tracks.ToList(), _selectedTrack));

            if (isPlaying)
            {
                await StartTrackAsync(nextTrack.Id);
            }
        }
    }

    /// <summary>
    /// Select previous track from album tracks
    /// </summary>
    public async Task PreviousTrack()
    {
        if (_selectedTrack != null)
        {
            var isPlaying = IsPlayingTrack(_selectedTrack.Id);
            var previousTrack = _tracks.SkipWhile(t => t != _selectedTrack).Skip(1).FirstOrDefault();
            
            if (previousTrack == null) return;
            
            SelectTrack(previousTrack.Id);
            PlayerStateChanged(new PlayerStateArgs(PlayerState.SkipPrevious, _album, _tracks.ToList(), _selectedTrack));
            
            if (isPlaying)
            {
                await StartTrackAsync(previousTrack.Id);
            }
        }
    }

    /// <summary>
    /// Check if track is selected or is selected with input guid
    /// </summary>
    /// <param name="trackId">Id of the track</param>
    /// <returns>True if is selected track</returns>
    public bool IsSelectedTrack(Guid? trackId)
    {
        if (!trackId.HasValue)
            return _selectedTrack != null;
        
        return _selectedTrack != null && _selectedTrack.Id == trackId;
    }
    
    /// <summary>
    /// Get currently selected track id
    /// </summary>
    /// <returns>Selected track id. Null if not found</returns>
    public Guid? GetSelectedTrackId()
    {
        return _selectedTrack?.Id;
    }

    /// <summary>
    /// Get currently selected track 
    /// </summary>
    /// <returns>Selected track. Null if not found</returns>
    public Track? GetSelectedTrack()
    {
        return _selectedTrack;
    }

    /// <summary>
    /// Get currently selected album
    /// </summary>
    /// <returns>Selected album. Null if not found</returns>
    public Album? GetSelectedAlbum()
    {
        return _album;
    }

    /// <summary>
    /// Get current play queue
    /// </summary>
    /// <returns></returns>
    public List<Track> GetTracks()
    {
        return _tracks.ToList();
    }

    /// <summary>
    /// Add single track to play queue
    /// </summary>
    /// <param name="track"></param>
    public void AddTrack(Track track)
    {
        _tracks.Add(track);

        if (!IsPlaying())
        {
            SelectTrack(track.Id);
            _ = StartTrackAsync(track.Id);
        }
    }

    /// <summary>
    /// Add more tracks to play queue
    /// </summary>
    /// <param name="playlistId"></param>
    /// <param name="tracks"></param>
    public void AddTracksFromPlaylist(List<Track> tracks)
    {
        foreach (var track in tracks)
            _tracks.Add(track);
    }

    /// <summary>
    /// Clear full queue
    /// </summary>
    public void ClearTracks()
    {
        _tracks.Clear();
    }

    /// <summary>
    /// Play input track
    /// </summary>
    /// <param name="track"></param>
    public void PlayTrack(Track track)
    {
        if (_tracks.Contains(track))
        {
            _ = StartTrackAsync(track.Id);
        }
    }

    /// <summary>
    /// Get input track state
    /// </summary>
    /// <param name="trackId"></param>
    /// <returns></returns>
    public PlayerState GetTrackState(Guid trackId)
    {
        if (_playingTrack?.Id == trackId)
        {
            if (IsPlaying())
                return PlayerState.Playing;
            return PlayerState.Paused;
        }

        if (_selectedTrack?.Id == trackId)
        {
            return PlayerState.Selected;
        }
        
        return PlayerState.None;
    }

    /// <summary>
    /// Check if player is playing something
    /// </summary>
    /// <returns></returns>
    public bool IsPlaying()
    {
        return _player?.State == PlaybackState.Playing;
    }

    /// <summary>
    /// Check if player is paused
    /// </summary>
    /// <returns></returns>
    public bool IsPaused()
    {
        return _player?.State == PlaybackState.Paused;
    }
    
    /// <summary>
    /// Check if trackId is playing
    /// </summary>
    /// <param name="trackId">Id of the track. If null, then checks if any track is playing</param>
    /// <param name="albumId">Id of the album if we want to check against that too</param>
    /// <returns>True if is playing</returns>
    public bool IsPlayingTrack(Guid? trackId, Guid? albumId = null)
    {
        if (_playingTrack != null)
        {
            if (trackId.HasValue && _playingTrack.Id == trackId.Value)
            {
                // Album id has value. Check against that too
                if (albumId.HasValue)
                {
                    return _album?.Id == albumId.Value;
                }
                
                return true;
            }
        }
        
        return false;
    }

    /// <summary>
    /// Get number where track is in queue
    /// </summary>
    /// <param name="trackId"></param>
    /// <returns></returns>
    public int? GetQueuePosition(Guid trackId)
    {
        if (_tracks.Any(t => t.Id == trackId))
            return _tracks.ToList().FindIndex(t => t.Id == trackId);
        
        return null;
    }
    
    /// <summary>
    /// Get album artwork if available
    /// </summary>
    /// <returns>Artwork, null if none found</returns>
    public byte[]? GetArtwork()
    {
        return _artwork;
    }

    public void Dispose()
    {
        if (_player != null)
        {
            _player.PlaybackEnded -= async (_, args) => await OnPlaybackEnded(_, args);
            _player.Stop();
            _player.Dispose();
            _player = null;
        }

        _device.Stop();
        _device.Dispose();
        _engine.Dispose();
    }
}