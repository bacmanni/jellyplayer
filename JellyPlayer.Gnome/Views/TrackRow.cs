using Adw.Internal;
using JellyPlayer.Gnome.Helpers;
using JellyPlayer.Shared.Models;
using System.Text.Encodings.Web;
using JellyPlayer.Shared.Enums;

namespace JellyPlayer.Gnome.Views;

public partial class TrackRow : Adw.ActionRow
{
    private readonly Track _track;
    
    [Gtk.Connect] private readonly Gtk.Image _status;
    [Gtk.Connect] private readonly Gtk.Label _runtime;
    [Gtk.Connect] private readonly Gtk.Label _number;
    [Gtk.Connect] private readonly Gtk.Button _queue;
    
    public Guid? GetTrackId() => _track.Id;
    
    private TrackRow(Gtk.Builder builder) : base(
        new ActionRowHandle(builder.GetPointer("_root"), false))
    {
        builder.Connect(this);
    }
    
    public TrackRow(Track track, PlayerState state) : this(Blueprint.BuilderFromFile("track_row"))
    {
        _track = track;
        
        if (_track.Number > 0)
            _number.SetText($"{_track.Number.ToString()}.");
        
        Activatable = true;

        if (_track.RunTime.HasValue)
            _runtime.SetText(_track.RunTime.Value.ToString("m\\:ss"));
        
        UpdateState(state);
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
        _status.SetFromIconName("media-playback-start-symbolic");
        SetTitle($"<b>{HtmlEncoder.Default.Encode(_track.Name)}</b>");
    }

    private void ClearTrack()
    {
        _status.SetFromIconName(null);
        SetTitle(HtmlEncoder.Default.Encode(_track.Name));
    }

    private void StopTrack()
    {
        _status.SetFromIconName("media-playback-pause-symbolic");
        SetTitle($"<b>{HtmlEncoder.Default.Encode(_track.Name)}</b>");
    }
}