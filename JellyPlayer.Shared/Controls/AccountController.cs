using JellyPlayer.Shared.Enums;
using JellyPlayer.Shared.Models;
using JellyPlayer.Shared.Services;

namespace JellyPlayer.Shared.Controls;

public sealed class AccountController
{
    private readonly IJellyPlayerApiService _jellyPlayerApiService;
    private readonly IConfigurationService _configurationService;
    private bool _isValid { get; set; }
    
    public bool IsValid() => _isValid;
    public string ServerUrl { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public bool RememberPassword { get; set; }
    public Guid? CollectionId { get; set; }
    public Guid? PlaylistCollectionId { get; set; }

    public event EventHandler<Configuration> OnConfigurationLoaded;

    public event EventHandler<bool> OnUpdate;
    
    public AccountController(IConfigurationService configurationService, IJellyPlayerApiService jellyPlayerApiService)
    {
        _jellyPlayerApiService = jellyPlayerApiService;
        _configurationService = configurationService;
    }

    /// <summary>
    /// Check if server is valid jellyfin server
    /// </summary>
    /// <param name="serverUrl"></param>
    /// <returns></returns>
    public async Task<bool> IsValidServer(string serverUrl)
    {
        if (Uri.IsWellFormedUriString(serverUrl, UriKind.Absolute))
        {
            return await _jellyPlayerApiService.CheckServerAsync(serverUrl);
        }
        
        return false;
    }

    /// <summary>
    /// Check that login was ok
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public async Task<bool> IsValidAccount(string username, string password)
    {
        if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
        {
            return await _jellyPlayerApiService.LoginAsync(username, password);
        }
        
        return false;
    }

    /// <summary>
    /// Get available collections
    /// </summary>
    /// <returns></returns>
    public async Task<List<Collection>> GetCollections(CollectionType type)
    {
        return await _jellyPlayerApiService.GetCollectionsAsync(type);
    }

    /// <summary>
    /// Get selected collection Id
    /// </summary>
    /// <returns></returns>
    public Guid? GetSelectedAudioCollectionId()
    {
        var id= _configurationService.Get()?.CollectionId;
        if (!Guid.TryParse(id, out var collectionId))
            return null;
        
        return collectionId;
    }

    /// <summary>
    /// Open input configuration
    /// </summary>
    /// <param name="configuration"></param>
    public void OpenConfiguration(Configuration configuration)
    {
        _isValid = true;
        ServerUrl = configuration.ServerUrl;
        Username = configuration.Username;
        Password = configuration.Password;
        RememberPassword = configuration.RememberPassword;
        CollectionId = Guid.Parse(configuration.CollectionId);
        
        if (configuration.PlaylistCollectionId != null)
            PlaylistCollectionId = Guid.Parse(configuration.PlaylistCollectionId);
        
        OnConfigurationLoaded?.Invoke(this, configuration);
    }

    /// <summary>
    /// Update controller validity
    /// </summary>
    /// <param name="server"></param>
    /// <param name="account"></param>
    /// <param name="collection"></param>
    public void UpdateValidity(bool server, bool account, bool collection)
    {
        if (server && account && collection)
        {
            _isValid = true;
        }
        else
        {
            _isValid = false;
        }
        
        OnUpdate?.Invoke(this, _isValid);
    }

    /// <summary>
    /// Check if account values are changed
    /// </summary>
    /// <returns></returns>
    public bool HasChanges()
    {
        var configuration = _configurationService.Get();

        if (configuration.ServerUrl != ServerUrl)
            return true;

        if (configuration.Username != Username)
            return true;
        
        if (configuration.Password != Password)
            return true;
        
        if (configuration.CollectionId != CollectionId?.ToString())
            return true;

        if (configuration.PlaylistCollectionId != PlaylistCollectionId?.ToString())
            return true;
            
        return false;
    }

    /// <summary>
    /// Get selected playlist collection id
    /// </summary>
    /// <returns></returns>
    public Guid? GetSelectedPlaylistCollectionId()
    {
        var id= _configurationService.Get().PlaylistCollectionId;
        if (!Guid.TryParse(id, out var collectionId))
            return null;
        
        return collectionId;
    }
}