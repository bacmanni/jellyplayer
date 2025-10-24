using Gtk.Internal;
using JellyPlayer.Gnome.Helpers;
using JellyPlayer.Shared.Controls;
using JellyPlayer.Shared.Events;
using ListBox = Gtk.ListBox;

namespace JellyPlayer.Gnome.Views;

public class PlaylistView : Gtk.Box
{
    private readonly PlaylistController _controller;
    
    [Gtk.Connect] private readonly Adw.Spinner _spinner;
    [Gtk.Connect] private readonly Adw.Clamp _results;
    [Gtk.Connect] private readonly Gtk.ListBox _playlistList;
    
    private PlaylistView(Gtk.Builder builder) : base(
        new BoxHandle(builder.GetPointer("_root"), false))
    {
        builder.Connect(this);
    }

    public PlaylistView(PlaylistController controller) : this(Blueprint.BuilderFromFile("playlist"))
    {
        _controller = controller;
        _controller.OnPlaylistStateChanged += ControllerOnPlaylistStateChanged;
        _playlistList.OnRowActivated += PlaylistListOnRowActivated;
        
        _results.SetVisible(false);
        _spinner.SetVisible(true);
    }

    private void PlaylistListOnRowActivated(ListBox sender, ListBox.RowActivatedSignalArgs args)
    {
        var row = args.Row as PlaylistRow;
        if (row != null)
        {
            _controller.OpenPlaylist(row.PlaylistId);
        }
    }

    private void ControllerOnPlaylistStateChanged(object? sender, PlaylistStateArgs e)
    {
        _playlistList.RemoveAll();
        
        foreach (var playlist in _controller.Playlists)
        {
            _playlistList.Append(new PlaylistRow(_controller.GetFileService(), playlist));
        }
        
        _spinner.SetVisible(false);
        _results.SetVisible(true);
    }

    public override void Dispose()
    {
        _controller.OnPlaylistStateChanged -= ControllerOnPlaylistStateChanged;
        _playlistList.OnRowActivated -= PlaylistListOnRowActivated;
        base.Dispose();
    }
}