using Gtk.Internal;
using JellyPlayer.Gnome.Helpers;
using JellyPlayer.Gnome.Models;
using JellyPlayer.Shared.Controls;
using JellyPlayer.Shared.Enums;
using JellyPlayer.Shared.Models;
using JellyPlayer.Shared.Services;

namespace JellyPlayer.Gnome.Views;

public class AlbumGridItem : Gtk.Box
{
    private readonly IFileService _fileService;
    
    [Gtk.Connect] private readonly Gtk.Image _albumArt;
    [Gtk.Connect] private readonly Gtk.Label _album;
    [Gtk.Connect] private readonly Gtk.Label _artist;
    
    private AlbumGridItem(Gtk.Builder builder) : base(
        new BoxHandle(builder.GetPointer("_root"), false))
    {
        builder.Connect(this);
    }
    
    public AlbumGridItem(IFileService fileService) : this(Blueprint.BuilderFromFile("album_grid_item"))
    {
        _fileService = fileService;
    }

    public async Task Bind(AlbumRow row)
    {
        _artist.SetLabel(row.Artist);
        _album.SetLabel(row.Album);
        
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