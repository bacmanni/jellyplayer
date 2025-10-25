namespace JellyPlayer.Shared.Events;

public class PlaylistStateArgs : EventArgs
{
    public Guid? PlaylistId { get; set; }
    public bool Loading { get; set; }
}