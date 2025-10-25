using Gtk.Internal;
using JellyPlayer.Gnome.Helpers;
using JellyPlayer.Shared.Controls;
using JellyPlayer.Shared.Events;
using ListBox = Gtk.ListBox;

namespace JellyPlayer.Gnome.Views;

public class AlbumView : Gtk.ScrolledWindow
{
    private readonly AlbumController _controller;
    
    [Gtk.Connect] private readonly Gtk.Image _albumArt;
    [Gtk.Connect] private readonly Gtk.Label _artist;
    [Gtk.Connect] private readonly Gtk.Label _album;
    [Gtk.Connect] private readonly Gtk.Label _trackCount;
    [Gtk.Connect] private readonly Gtk.Label _albumDuration;
    [Gtk.Connect] private readonly Gtk.Label _albumYear;
    [Gtk.Connect] private readonly Gtk.ListBox _tracks;
    [Gtk.Connect] private readonly Adw.Spinner _spinner;
    [Gtk.Connect] private readonly Adw.Clamp _result;

    private bool _isCtrlActive = false;
    
    private AlbumView(Gtk.Builder builder) : base(
        new ScrolledWindowHandle(builder.GetPointer("_root"), false))
    {
        builder.Connect(this);
    }
    
    public AlbumView(AlbumController controller) : this(Blueprint.BuilderFromFile("album"))
    {
        _controller = controller;
        
        _tracks.OnRowSelected += TracksOnRowSelected;
        _tracks.OnRowActivated += TracksOnRowActivated;
        _controller.OnAlbumChanged += ControllerOnAlbumChanged;
    }

    private void ControllerOnAlbumChanged(object? sender, AlbumStateArgs args)
    {
        switch (args.UpdateAlbum)
        {
            case false when !args.UpdateTracks && !args.UpdateArtwork && !args.UpdateTrackState:
                SetSpinner(true);
                break;
            case true:
                UpdateAlbum();
                break;
        }

        if (args.UpdateTracks)
            UpdateTracks();

        if (args.UpdateArtwork)
            UpdateArtwork();

        if (args.UpdateTrackState)
            UpdateTrackState(args.SelectedTrackId.Value);
    }

    private void TracksOnRowActivated(ListBox sender, ListBox.RowActivatedSignalArgs args)
    {
        if (args.Row is TrackRow row && row.GetTrackId().HasValue)
        {
            if (_isCtrlActive)
            {
                _controller.AddTrackToQueue(row.GetTrackId().Value);
            }
            else
            {
                _controller.PlayOrPauseTrack(row.GetTrackId().Value);
            }
        }
    }

    private void TracksOnRowSelected(ListBox sender, ListBox.RowSelectedSignalArgs args)
    {
        if (args.Row is TrackRow row && row.GetTrackId().HasValue)
        {
            _controller.SelectTrack(row.GetTrackId().Value);
        }
    }

    private void SetSpinner(bool show)
    {
        if (show)
        {
            _result.SetVisible(false);
            _spinner.SetVisible(true);
        }
        else
        {
            _spinner.SetVisible(false);
            _result.SetVisible(true);
        }
    }
    
    private void UpdateAlbum()
    {
        // Clear artwork
        _albumArt.Clear();

        SetSpinner(false);
        
        _artist.SetText(_controller.Album.Artist);
        _album.SetText(_controller.Album.Name);
        _trackCount.SetText($"{_controller.Tracks.Count.ToString()} tracks");
        
        if (_controller.Album?.Runtime != null)
            _albumDuration.SetText($"{_controller.Album.Runtime.Value.TotalMinutes:F0}m");
        
        if (_controller.Album?.Year != null)
            _albumYear.SetText(_controller.Album.Year.Value.ToString());
    }

    private void UpdateArtwork()
    {
        if ( _controller.Artwork != null)
        {
            using var bytes = GLib.Bytes.New(_controller.Artwork);
            using var texture = Gdk.Texture.NewFromBytes(bytes);
            _albumArt.SetFromPaintable(texture);
        }
    }
    
    private void UpdateTracks()
    {
        _tracks.RemoveAll();
        foreach (var track in _controller.Tracks)
        {
            var state = _controller.GetPlayerService().GetTrackState(track.Id);
            var row = new TrackRow(track, state);
            _tracks.Append(row);
        }
    }

    private void UpdateTrackState(Guid trackId)
    {
        for (var i = 0; i < _controller.Tracks.Count; i++)
        {
            var row = _tracks.GetRowAtIndex(i) as TrackRow;
            if (row == null)  continue;

            var rowTrackId = row.GetTrackId() ?? throw new NullReferenceException();
            var state = _controller.GetPlayerService().GetTrackState(rowTrackId);
            row.UpdateState(state);
        }
    }

    public void IsCtrlActive(bool active)
    {
        _isCtrlActive = active;
    }

    public override void Dispose()
    {
        _tracks.OnRowSelected -= TracksOnRowSelected;
        _tracks.OnRowActivated -= TracksOnRowActivated;
        _controller.OnAlbumChanged -= ControllerOnAlbumChanged;
        base.Dispose();
    }
}