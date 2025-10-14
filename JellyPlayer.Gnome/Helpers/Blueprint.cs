using System.IO;
using System.Reflection;
using System.Xml;

namespace JellyPlayer.Gnome.Helpers;

public abstract class Blueprint
{
    public static Gtk.Builder BuilderFromFile(string name)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("JellyPlayer.Gnome.Blueprints." + name + ".ui");
        using var reader = new StreamReader(stream!);
        var uiContents = reader.ReadToEnd();
        var xml = new XmlDocument();
        xml.LoadXml(uiContents);

        try
        {
            return Gtk.Builder.NewFromString(xml.OuterXml, -1);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
    }

    public static GLib.Bytes BytesFromFile(string name)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("JellyPlayer.Gnome.Blueprints." + name + ".ui");
        using var memStream = new MemoryStream();
        using var reader = new StreamReader(stream!);
        reader.BaseStream.CopyTo(memStream);
        var bytes = memStream.ToArray();
        return GLib.Bytes.New(bytes);
    }
}