using System.Text.Encodings.Web;
using Adw.Internal;
using JellyPlayer.Gnome.Helpers;
using JellyPlayer.Shared.Enums;
using JellyPlayer.Shared.Models;

namespace JellyPlayer.Gnome.Views;

public class QueueRow : Adw.ActionRow
{
    private readonly Track _track;

    [Gtk.Connect] private readonly Gtk.Image _status;
    [Gtk.Connect] private readonly Gtk.Image _albumArt;
    [Gtk.Connect] private readonly Gtk.Label _runtime;

    private QueueRow(Gtk.Builder builder) : base(
        new ActionRowHandle(builder.GetPointer("_root"), false))
    {
        builder.Connect(this);
    }
    
    public QueueRow(Track track, PlayerState state) : this(Blueprint.BuilderFromFile("queue_row"))
    {
        _track  = track;
        
        SetTitle(track.Name);
        SetSubtitle(track.Artist);
        Activatable = true;
        
        if (_track.RunTime.HasValue)
            _runtime.SetText(_track.RunTime.Value.ToString("m\\:ss"));

        UpdateState(state);
    }

    public Guid GetTrackId()
    {
        return _track.Id;
    }
    
    public void UpdateState(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.Playing:
                StartTrack();
                break;
            case PlayerState.Paused:
                StopTrack();
                break;
            default:
                ClearTrack();
                break;
        }
    }
    
    private void StartTrack()
    {
        _albumArt.SetVisible(false);
        _status.SetVisible(true);
        _status.SetFromIconName("media-playback-start-symbolic");
        SetTitle($"<b>{HtmlEncoder.Default.Encode(_track.Name)}</b>");
    }

    private void ClearTrack()
    {
        _status.SetVisible(false);
        _albumArt.SetVisible(true);
        SetTitle(HtmlEncoder.Default.Encode(_track.Name));
    }

    private void StopTrack()
    {
        _albumArt.SetVisible(false);
        _status.SetVisible(true);
        _status.SetFromIconName("media-playback-pause-symbolic");
        SetTitle($"<b>{HtmlEncoder.Default.Encode(_track.Name)}</b>");
    }
}