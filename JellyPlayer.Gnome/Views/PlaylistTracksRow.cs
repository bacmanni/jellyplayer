using System.Text.Encodings.Web;
using Adw.Internal;
using JellyPlayer.Gnome.Helpers;
using JellyPlayer.Shared.Enums;
using JellyPlayer.Shared.Models;
using JellyPlayer.Shared.Services;

namespace JellyPlayer.Gnome.Views;

public class PlaylistTracksRow : Adw.ActionRow
{
    private readonly IFileService _fileService;
    private readonly Track  _track;
    
    [Gtk.Connect] private readonly Gtk.Image _albumArt;
    [Gtk.Connect] private readonly Gtk.Label _duration;
    
    public Guid TrackId => _track.Id;
    
    private PlaylistTracksRow(Gtk.Builder builder) : base(
        new ActionRowHandle(builder.GetPointer("_root"), false))
    {
        builder.Connect(this);
    }

    public PlaylistTracksRow(IFileService fileService, Track track, PlayerState state) : this(
        Blueprint.BuilderFromFile("playlist_tracks_row"))
    {
        _track = track;
        _fileService = fileService;
        Activatable = true;

        SetTitle(_track.Name);
        SetSubtitle(_track.Artist);
        
        if (_track.RunTime.HasValue)
            _duration.SetText(_track.RunTime.Value.ToString("m\\:ss"));

        if (_track.HasArtwork)
            UpdateArtwork();
    }
    
    private async Task UpdateArtwork()
    {
        var albumArt = await _fileService.GetFileAsync(FileType.AlbumArt, _track.AlbumId);
        if  (albumArt == null || albumArt.Length == 0)
            return;
        
        using var bytes = GLib.Bytes.New(albumArt);
        using var texture = Gdk.Texture.NewFromBytes(bytes);
        _albumArt.SetFromPaintable(texture);
    }
}