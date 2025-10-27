using System.Reflection;

namespace JellyPlayer.Gnome.Helpers;

public abstract class Blueprint
{
    public static Gtk.Builder BuilderFromFile(string name)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("JellyPlayer.Gnome.Blueprints." + name + ".ui");
        using var reader = new StreamReader(stream!);
        var uiContents = reader.ReadToEnd();

        try
        {
            return Gtk.Builder.NewFromString(uiContents, -1);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
    }
}