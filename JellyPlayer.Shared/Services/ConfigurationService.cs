using System.Runtime.InteropServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using JellyPlayer.Shared.Models;

namespace JellyPlayer.Shared.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly Configuration _configuration = new();
    
    /// <summary>
    /// Occurs when the configuration object is saved
    /// </summary>
    public event EventHandler<EventArgs>? Saved;

    /// <summary>
    /// Occurs when the configuration object is loaded
    /// </summary>
    public event EventHandler<EventArgs>? Loaded;
    
    /// <summary>
    /// Saves the configuration file
    /// </summary>
    public void Save()
    {
        var filename = GetFilename();
        var json = JsonSerializer.Serialize(_configuration,  options: new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) });
        File.WriteAllText(filename, json);
        
        Saved?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Load configuration from file
    /// </summary>
    public void Load()
    {
        var filename = GetFilename();

        if (!File.Exists(filename))
        {
            CreateConfigurationFile(filename);
        }
        
        var json = File.ReadAllText(filename);
        
        if (!string.IsNullOrEmpty(json))
        {
            var configuration = JsonSerializer.Deserialize<Configuration>(json);

            if (configuration != null)
            {
                var properties = typeof(Configuration).GetProperties();
                foreach (var property in properties)
                {
                    property.SetValue(_configuration, property.GetValue(configuration));
                }
            }
        }
        
        Loaded?.Invoke(this, EventArgs.Empty);
    }

    public string GetConfigurationDirectory()
    {
        var platform = GetOsPlatform();
        if (platform == OSPlatform.Linux)
        {
            return $"/home/{Environment.UserName}/.jellyplayer/";
        }
        else if (platform == OSPlatform.OSX)
        {
            return $"/Users/{Environment.UserName}/.jellyplayer/";
        }
        
        throw new PlatformNotSupportedException();
    }

    /// <summary>
    /// Get stored configuration
    /// </summary>
    /// <returns></returns>
    public Configuration Get()
    {
        return _configuration;
    }

    /// <summary>
    /// Update configuration
    /// </summary>
    /// <param name="configuration"></param>
    public void Set(Configuration configuration)
    {
        var properties = typeof(Configuration).GetProperties();
        foreach (var property in properties)
        {
            property.SetValue(_configuration, property.GetValue(configuration));
        }
    }

    private void CreateConfigurationFile(string filename)
    {
        try
        {
            if (!Directory.Exists(filename))
            {
                (new FileInfo(filename)).Directory.Create();
            }
            
            if (!File.Exists(filename))
            {
                var file = new FileInfo(filename);
                file.Directory.Create();
                Save();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    private string GetFilename()
    {
        var platform = GetOsPlatform();
        if (platform == OSPlatform.Linux)
        {
            return $"{GetConfigurationDirectory()}configuration.json";
        }
        else if (platform == OSPlatform.OSX)
        {
            return $"{GetConfigurationDirectory()}configuration.json";
        }
        else if (platform == OSPlatform.Windows)
        {
            return $"{GetConfigurationDirectory()}configuration.json";
        }
        
        throw new PlatformNotSupportedException();
    }
    private OSPlatform GetOsPlatform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return OSPlatform.Windows;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return OSPlatform.Linux;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return OSPlatform.OSX;

        throw new Exception("Unsupported OS Platform");
    }
}