using JellyPlayer.Shared.Events;

namespace JellyPlayer.Shared.Controls;

public class PlaylistTracksController
{
    public event EventHandler<PlaylistTracksStateArgs> OnPlaylistClicked;
    
    public async Task OpenPlaylist(Guid playlistId)
    {
        
    }
}