using Adw.Internal;
using JellyPlayer.Gnome.Helpers;
using JellyPlayer.Shared.Controls;

namespace JellyPlayer.Gnome.Views;

public partial class LyricsView : Adw.Dialog
{
    private LyricsController  _controller;

    [Gtk.Connect] private readonly Adw.Spinner _spinner;
    [Gtk.Connect] private readonly Gtk.Box _results;
    
    [Gtk.Connect] private readonly Gtk.Label _lyrics;
    [Gtk.Connect] private readonly Gtk.Image _albumArt;
    [Gtk.Connect] private readonly Gtk.Label _track;
    [Gtk.Connect] private readonly Gtk.Label _artist;
    [Gtk.Connect] private readonly Gtk.Button _update;
    
    private LyricsView(Gtk.Builder builder) : base(
        new DialogHandle(builder.GetPointer("_root"), false))
    {
        builder.Connect(this);
    }
    
    public LyricsView(LyricsController controller) : this(Blueprint.BuilderFromFile("lyrics"))
    {
        _controller = controller;
        _controller.OnLyricsUpdated += ControllerOnOnLyricsUpdated;
        _results.SetVisible(false);
        _spinner.SetVisible(true);
    }

    private void ControllerOnOnLyricsUpdated(object? sender, EventArgs e)
    {
        _track.SetLabel(_controller.TrackName);
        _artist.SetLabel(_controller.ArtistName);
        
        if (!string.IsNullOrWhiteSpace(_controller.Lyrics))
            _lyrics.SetLabel(_controller.Lyrics);;

        if (_controller?.AlbumArt == null) return;
        using var bytes = GLib.Bytes.New(_controller.AlbumArt);
        using var texture = Gdk.Texture.NewFromBytes(bytes);
        _albumArt.SetFromPaintable(texture);
        
        _spinner.SetVisible(false);
        _results.SetVisible(true);
    }
}
