using Gtk.Internal;
using JellyPlayer.Gnome.Helpers;
using JellyPlayer.Gnome.Models;
using JellyPlayer.Shared.Controls;
using JellyPlayer.Shared.Enums;
using JellyPlayer.Shared.Models;
using SignalListItemFactory = Gtk.SignalListItemFactory;

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
        _controller.OnQueueUpdated += (sender, args) =>
        {
            _queueList.RemoveAll();
            foreach (var track in _controller.Tracks)
            {
                var state = _controller.GetPlayerService().GetTrackState(track.Id);
                var row = new QueueRow(track, state);
                _queueList.Append(new QueueRow(track, state));
            }
        };
        
        _queueList.OnRowActivated += (box, signalArgs) =>
        {
            var row = signalArgs.Row as QueueRow;
            if (row is null)
                return;
                
            _controller.GetPlayerService().StartTrackAsync(row.GetTrackId());
        };

        _controller.GetPlayerService().OnPlayerStateChanged += (sender, args) =>
        {
            if (args.State is PlayerState.Playing or PlayerState.Paused)
            {
                UpdateRowState(args.SelectedTrack.Id, args.State);
            }
        };
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
}