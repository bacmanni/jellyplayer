using Gtk.Internal;
using JellyPlayer.Gnome.Helpers;
using JellyPlayer.Shared.Controls;
using JellyPlayer.Shared.Events;

namespace JellyPlayer.Gnome.Views;

public class PlaylistView : Gtk.Box
{
    private readonly PlaylistController _controller;
    
    [Gtk.Connect] private readonly Adw.Spinner _spinner;
    [Gtk.Connect] private readonly Adw.NavigationSplitView _playlist_view;
    
    private PlaylistView(Gtk.Builder builder) : base(
        new BoxHandle(builder.GetPointer("_root"), false))
    {
        builder.Connect(this);
    }

    public PlaylistView(PlaylistController controller) : this(Blueprint.BuilderFromFile("playlist"))
    {
        _controller = controller;
        _controller.OnPlaylistStateChanged += ControllerOnPlaylistStateChanged;
        _playlist_view.SetVisible(false);
        _spinner.SetVisible(true);
    }

    private void ControllerOnPlaylistStateChanged(object? sender, PlaylistStateArgs e)
    {
        _spinner.SetVisible(false);
        _playlist_view.SetVisible(true);
    }

    public override void Dispose()
    {
        _controller.OnPlaylistStateChanged -= ControllerOnPlaylistStateChanged;
        base.Dispose();
    }
}