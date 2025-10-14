using Gio;
using GLib;

namespace JellyPlayer.Gnome.Controls;

/// <summary>
/// This will control media player widget in menu
/// </summary>
public class MediaPlayerController
{
    /// <summary>
    /// Creates the player
    /// </summary>
    public void CreatePlayer()
    {
        var bus = DBusConnection.Get(BusType.Session);
        using var parameters = Variant.NewTuple(new[] {
            Variant.NewString("AppName"),
            Variant.NewUint32(0u),
            Variant.NewString(""), //Icon
            Variant.NewString("Summary"),
            Variant.NewString("Body"),
            Variant.NewStrv(System.Array.Empty<string>(), 0),
            Variant.NewArray(VariantType.NewDictEntry(VariantType.New("s"), VariantType.New("v")), null), //hints
            Variant.NewInt32(999)
        });
    }
}