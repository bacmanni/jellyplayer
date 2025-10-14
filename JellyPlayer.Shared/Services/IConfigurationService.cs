using JellyPlayer.Shared.Models;

namespace JellyPlayer.Shared.Services;

public interface IConfigurationService
{
    public event EventHandler<EventArgs>? Saved;
    public event EventHandler<EventArgs>? Loaded;
    public void Save();
    public void Load();
    public string GetConfigurationDirectory();
    Configuration Get();
    void Set(Configuration configuration);
}