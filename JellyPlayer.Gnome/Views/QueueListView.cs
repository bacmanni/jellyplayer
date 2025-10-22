using Gtk.Internal;
using JellyPlayer.Gnome.Helpers;
using JellyPlayer.Shared.Controls;
using JellyPlayer.Shared.Enums;
using JellyPlayer.Shared.Events;
using JellyPlayer.Shared.Models;
using ListBox = Gtk.ListBox;

namespace JellyPlayer.Gnome.Views;

public class QueueListView : Gtk.ScrolledWindow
{
    private readonly QueueListController  _controller;
    
    [Gtk.Connect] private readonly Gtk.ListBox _queueList;

    private QueueListView(Gtk.Builder builder) : base(
        new ScrolledWindowHandle(builder.GetPointer("_root"), false))
    {
        builder.Connect(this);
    }

    public QueueListView(QueueListController controller) : this(Blueprint.BuilderFromFile("queue_list"))
    {
        _controller = controller;
        _controller.OnQueueUpdated += ControllerOnQueueUpdated;
        _queueList.OnRowActivated += QueueListOnRowActivated;
        _controller.GetPlayerService().OnPlayerStateChanged += OnPlayerStateChanged;
    }

    private void OnPlayerStateChanged(object? sender, PlayerStateArgs args)
    {
        if (args.State is PlayerState.Playing or PlayerState.Paused)
        {
            UpdateRowState(args.SelectedTrack.Id, args.State);
        }
    }

    private void QueueListOnRowActivated(ListBox sender, ListBox.RowActivatedSignalArgs args)
    {
        var row = args.Row as QueueRow;
        if (row is null)
            return;
                
        _controller.GetPlayerService().StartTrackAsync(row.GetTrackId());
    }

    private void ControllerOnQueueUpdated(object? sender, QueueArgs e)
    {
        _queueList.RemoveAll();
        foreach (var track in _controller.Tracks)
        {
            var state = _controller.GetPlayerService().GetTrackState(track.Id);
            var row = new QueueRow(track, state);
            _queueList.Append(new QueueRow(track, state));
        }
    }

    private void UpdateRowState(Guid trackId, PlayerState state)
    {
        for (var i = 0; i < _controller.Tracks.Count; i++)
        {
            var row = _queueList.GetRowAtIndex(i) as QueueRow;
            if (row == null)  continue;

            row.UpdateState(row.GetTrackId() == trackId ? state : PlayerState.None);
        }
    }

    public override void Dispose()
    {
        _controller.OnQueueUpdated -= ControllerOnQueueUpdated;
        _queueList.OnRowActivated -= QueueListOnRowActivated;
        _controller.GetPlayerService().OnPlayerStateChanged -= OnPlayerStateChanged;
        base.Dispose();
    }
}