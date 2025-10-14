using Adw.Internal;
using JellyPlayer.Gnome.Helpers;

namespace JellyPlayer.Gnome.Views;

public class PreferencesAlert : Adw.AlertDialog
{
    private PreferencesAlert(Gtk.Builder builder) : base(
        new AlertDialogHandle(builder.GetPointer("_root"), false))
    {
        builder.Connect(this);
    }

    public PreferencesAlert() : this(Blueprint.BuilderFromFile("preferences_alert"))
    {
    }
}