using Gtk.Internal;
using JellyPlayer.Gnome.Helpers;
using JellyPlayer.Shared.Controls;

namespace JellyPlayer.Gnome.Views;

public class PlaylistTracksView : Gtk.Box
{
    private PlaylistTracksView(Gtk.Builder builder) : base(
        new BoxHandle(builder.GetPointer("_root"), false))
    {
        builder.Connect(this);
    }

    public PlaylistTracksView(PlaylistTracksController controller) : this(Blueprint.BuilderFromFile("playlist_tracks"))
    {
    }
}