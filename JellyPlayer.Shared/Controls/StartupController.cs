using JellyPlayer.Shared.Enums;
using JellyPlayer.Shared.Models;
using JellyPlayer.Shared.Services;

namespace JellyPlayer.Shared.Controls;

/// <summary>
/// This controller is shared by login
/// </summary>
public class StartupController : IDisposable
{
    private readonly IJellyPlayerApiService _jellyPlayerApiService;
    private readonly IConfigurationService _configurationService;

    public IConfigurationService GetConfigurationService() => _configurationService;
    
    public IJellyPlayerApiService  GetJellyPlayerApiService() => _jellyPlayerApiService;
    
    public StartupController(IJellyPlayerApiService jellyPlayerApiService, IConfigurationService configurationService)
    {
        _jellyPlayerApiService = jellyPlayerApiService;
        _configurationService = configurationService;
    }

    /// <summary>
    /// This is the startup method. It will check that required data is saved.
    /// </summary>
    /// <returns></returns>
    public async Task<StartupState> StartAsync(string? nonStoredPassword = null)
    {
        var configuration = _configurationService.Get();
        
        var server = configuration.ServerUrl;
        var username = configuration.Username;
        var password = !string.IsNullOrWhiteSpace(nonStoredPassword) ? nonStoredPassword : configuration.Password;
        var collectionId = configuration.CollectionId;
        
        // This should only happen when no configuration is saved
        if (string.IsNullOrEmpty(server) && string.IsNullOrEmpty(username) && string.IsNullOrEmpty(password))
        {
            return StartupState.InitialRun;
        }
        
        var success= _jellyPlayerApiService.SetServer(server);
        if (!success)
        {
            return StartupState.InvalidServer;
        }

        var isSupportedServer = await _jellyPlayerApiService.CheckServerAsync(server);
        if (!isSupportedServer)
        {
            return StartupState.InvalidServer;
        }
        
        if (string.IsNullOrEmpty(password))
        {
            return StartupState.RequirePassword;
        }
        
        var logged = await _jellyPlayerApiService.LoginAsync(username, password);
        if (!logged)
        {
            return StartupState.AccountProblem;
        }
        
        var collections = await _jellyPlayerApiService.GetCollectionsAsync(CollectionType.Audio);
        if (collections.Count == 0)
        {
            return StartupState.MissingCollection;
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(collectionId))
            {
                var id = Guid.Parse(collectionId);
                var collection = collections.FirstOrDefault(c => c.Id == id);
                if (collection != null)
                {
                    _jellyPlayerApiService.SetCollectionId(collection.Id);
                    return StartupState.Finished;
                }
            }
            
            return StartupState.SelectCollection;
        }
    }

    /// <summary>
    /// Save configuration
    /// </summary>
    /// <param name="configuration"></param>
    public void SaveConfiguration(Configuration configuration)
    {
        _configurationService.Set(configuration);
        _configurationService.Save();
    }

    public void Dispose()
    {

    }
}