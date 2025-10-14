using Adw.Internal;
using JellyPlayer.Gnome.Helpers;
using JellyPlayer.Shared.Enums;
using JellyPlayer.Shared.Models;
using JellyPlayer.Shared.Services;

namespace JellyPlayer.Gnome.Views;

public class SearchRow : Adw.ActionRow
{
    private readonly IFileService  _fileService;
    public Guid Id  { get; set; }
    public Guid AlbumId  { get; set; }
    public SearchType Type { get; set; }
    
    [Gtk.Connect] private readonly Gtk.Image _albumArt;
    [Gtk.Connect] private readonly Gtk.Label _search_item_title;
    [Gtk.Connect] private readonly Gtk.Label _search_item_description;

    private SearchRow(Gtk.Builder builder) : base(
        new ActionRowHandle(builder.GetPointer("_root"), false))
    {
        builder.Connect(this);
    }

    public SearchRow(IFileService fileService, Search row) : this(Blueprint.BuilderFromFile("search_row"))
    {
        _fileService = fileService;
        Id = row.Id;
        AlbumId  = row.AlbumId;
        
        Activatable = true;
        
        switch (row.Type)
        {
            case SearchType.Album or SearchType.Artist:
                _search_item_title.SetLabel(row.AlbumName);
                break;
            default:
                _search_item_title.SetLabel(row.TrackName);
                break;
        }
        
        var description = $"by {row.ArtistName}";
        if (row.Type == SearchType.Track)
            description += $" on {row.AlbumName}";
        
        _search_item_description.SetLabel(description);
        
        if (!row.HasArtwork)
            return;

        UpdateArtwork();
    }

    private async Task UpdateArtwork()
    {
        var albumArt = await _fileService.GetFileAsync(FileType.AlbumArt, AlbumId);
        if  (albumArt == null || albumArt.Length == 0)
            return;
        
        using var bytes = GLib.Bytes.New(albumArt);
        using var texture = Gdk.Texture.NewFromBytes(bytes);
        _albumArt.SetFromPaintable(texture);
    }
}