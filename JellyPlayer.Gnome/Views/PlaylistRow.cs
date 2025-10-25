using Adw.Internal;
using JellyPlayer.Gnome.Helpers;
using JellyPlayer.Shared.Enums;
using JellyPlayer.Shared.Models;
using JellyPlayer.Shared.Services;

namespace JellyPlayer.Gnome.Views;

public partial class PlaylistRow : Adw.ActionRow
{
    private readonly IFileService _fileService;
    private readonly Playlist  _playlist;
    
    [Gtk.Connect] private readonly Gtk.Image _playlist_primary_image;
    [Gtk.Connect] private readonly Gtk.Label _playlist_item_title;
    [Gtk.Connect] private readonly Gtk.Label _playlist_item_description;
    
    public readonly Guid PlaylistId;
    
    private PlaylistRow(Gtk.Builder builder) : base(
        new ActionRowHandle(builder.GetPointer("_root"), false))
    {
        builder.Connect(this);
    }

    public PlaylistRow(IFileService fileService, Playlist playlist) : this(Blueprint.BuilderFromFile("playlist_row"))
    {
        _playlist  = playlist;
        _fileService = fileService;
        PlaylistId = playlist.Id;
        
        Activatable = true;
        
        _playlist_item_title.SetText(_playlist.Name);

        var description = $"{_playlist.TrackCount.ToString()} tracks";
        if (_playlist.Duration.HasValue)
            description += $", duration {_playlist.Duration.Value.ToString("m\\:ss")}";
        
        _playlist_item_description.SetText(description);

        if (_playlist.HasArtwork)
            UpdateArtwork();
    }
    
    private async Task UpdateArtwork()
    {
        var artWork = await _fileService.GetFileAsync(FileType.Playlist, _playlist.Id);
        if  (artWork == null || artWork.Length == 0)
            return;
        
        using var bytes = GLib.Bytes.New(artWork);
        using var texture = Gdk.Texture.NewFromBytes(bytes);
        _playlist_primary_image.SetFromPaintable(texture);
    }
}