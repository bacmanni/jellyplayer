using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Runtime.InteropServices;

namespace JellyPlayer.Shared.Models;
public class Configuration
{
    /// <summary>
    /// Generated device id. Generated when configuration is created
    /// </summary>
    public string DeviceId { get; set; } = string.Empty;
    
    /// <summary>
    /// Server to be used
    /// </summary>
    public string ServerUrl { get; set; } = string.Empty;

    /// <summary>
    /// Username for the server
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Password for the server
    /// </summary>
    public string Password { get; set; } = string.Empty;
    
    /// <summary>
    /// Should password be remembered in configuration
    /// </summary>
    public bool RememberPassword { get; set; } = false;
    
    /// <summary>
    /// Selected audio collection id
    /// </summary>
    public string CollectionId { get; set; } = string.Empty;
    
    /// <summary>
    /// Selected playlist collection (Optional)
    /// </summary>
    public string? PlaylistCollectionId { get; set; } = null;

    /// <summary>
    /// Should album art be stored on disk
    /// </summary>
    public bool CacheAlbumArt { get; set; } = false;
    
    /// <summary>
    /// Should list separator be visible
    /// </summary>
    public bool ShowListSeparator {  get; set; } = false;
}