using Gtk.Internal;
using JellyPlayer.Gnome.Helpers;
using JellyPlayer.Shared.Controls;
using JellyPlayer.Shared.Enums;
using JellyPlayer.Shared.Events;
using ListBox = Gtk.ListBox;

namespace JellyPlayer.Gnome.Views;

public class PlaylistTracksView : Gtk.Box
{
    private readonly PlaylistTracksController _controller;
    
    [Gtk.Connect] private readonly Adw.Spinner _spinner;
    [Gtk.Connect] private readonly Adw.Clamp _results;
    [Gtk.Connect] private readonly Gtk.ListBox _playlistTracksList;
    
    private PlaylistTracksView(Gtk.Builder builder) : base(
        new BoxHandle(builder.GetPointer("_root"), false))
    {
        builder.Connect(this);
    }

    public PlaylistTracksView(PlaylistTracksController controller) : this(Blueprint.BuilderFromFile("playlist_tracks"))
    {
        _controller = controller;
        _controller.OnPlaylistTracksStateChanged += ControllerOnPlaylistTracksStateChanged;
        _playlistTracksList.OnRowActivated += PlaylistTracksListOnRowActivated;
        
        _results.SetVisible(false);
        _spinner.SetVisible(true);
    }

    private void PlaylistTracksListOnRowActivated(ListBox sender, ListBox.RowActivatedSignalArgs args)
    {
        if (args.Row is PlaylistTracksRow row)
        {
            _controller.PlayOrPauseTrack(row.TrackId);
        }
    }

    private void ControllerOnPlaylistTracksStateChanged(object? sender, PlaylistTracksStateArgs e)
    {
        if (e.Loading)
        {
            _results.SetVisible(false);
            _spinner.SetVisible(true);
            return;
        }
        
        _playlistTracksList.RemoveAll();
        
        foreach (var track in _controller.Tracks)
        {
            var state = _controller.GetPlayerService().GetTrackState(track.Id);
            _playlistTracksList.Append(new PlaylistTracksRow(_controller.GetFileService(), track, state));
        }
        
        _spinner.SetVisible(false);
        _results.SetVisible(true);
    }

    public override void Dispose()
    {
        _controller.OnPlaylistTracksStateChanged -= ControllerOnPlaylistTracksStateChanged;
        _playlistTracksList.OnRowActivated -= PlaylistTracksListOnRowActivated;
        base.Dispose();
    }
}