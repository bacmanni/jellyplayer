using JellyPlayer.Shared.Enums;

namespace JellyPlayer.Shared.Models;

public class Playlist
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public PlaylistType Type { get; set; }
}