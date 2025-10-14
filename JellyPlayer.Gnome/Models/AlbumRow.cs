using GObject;
using Gtk;
using JellyPlayer.Shared.Models;

namespace JellyPlayer.Gnome.Models;

[Subclass<GObject.Object>]
public partial class AlbumRow
{
    public Guid Id  { get; set; }
    public string Artist { get; set; }
    public string Album { get; set; }
    public bool HasArtwork { get; set; }
    
    public AlbumRow(Album album) : this()
    {
        Update(album);
    }

    public void Update(Album album)
    {
        Id = album.Id;
        Artist = album.Artist;
        Album = album.Name;
        HasArtwork = album.HasArtwork;
    }
}