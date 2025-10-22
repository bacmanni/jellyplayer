using Gtk.Internal;
using JellyPlayer.Gnome.Helpers;
using JellyPlayer.Gnome.Models;
using JellyPlayer.Shared.Enums;
using JellyPlayer.Shared.Services;

namespace JellyPlayer.Gnome.Views;

public class AlbumListItem : Gtk.Box
{
    private readonly IFileService _fileService;
    
    [Gtk.Connect] private readonly Gtk.Image _albumArt;
    [Gtk.Connect] private readonly Gtk.Label _album;
    [Gtk.Connect] private readonly Gtk.Label _artist;
    
    private AlbumListItem(Gtk.Builder builder) : base(
        new BoxHandle(builder.GetPointer("_root"), false))
    {
        builder.Connect(this);
    }
    
    public AlbumListItem(IFileService fileService) : this(Blueprint.BuilderFromFile("album_list_item"))
    {
        _fileService = fileService;
    }

    public async Task Bind(AlbumRow row)
    {
        _album.SetLabel(row.Album);
        _artist.SetLabel(row.Artist);
        
        if (!row.HasArtwork)
            return;
            
        var albumArt = await _fileService.GetFileAsync(FileType.AlbumArt, row.Id);
        if  (albumArt == null || albumArt.Length == 0)
            return;
        
        using var bytes = GLib.Bytes.New(albumArt);
        using var texture = Gdk.Texture.NewFromBytes(bytes);
        _albumArt.SetFromPaintable(texture);
    }

    public void Clear()
    {
        _albumArt.Clear();
    }
}
