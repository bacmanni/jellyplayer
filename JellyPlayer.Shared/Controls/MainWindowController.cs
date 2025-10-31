using JellyPlayer.Shared.Models;
using JellyPlayer.Shared.Services;

namespace JellyPlayer.Shared.Controls;

public sealed class MainWindowController : IDisposable
{
    private readonly IJellyPlayerApiService _jellyPlayerApiService;
    private readonly IConfigurationService _configurationService;
    private readonly IPlayerService _playerService;
    private readonly IFileService _fileService;
    public readonly ApplicationInfo ApplicationInfo;
    
    public IConfigurationService GetConfigurationService() => _configurationService;
    public IJellyPlayerApiService GetJellyPlayerApiService() => _jellyPlayerApiService;
    public IPlayerService GetPlayerService() => _playerService;
    public IFileService GetFileService() => _fileService;

    public MainWindowController(IJellyPlayerApiService jellyPlayerApiService, IConfigurationService configurationService, IPlayerService playerService, IFileService fileService, ApplicationInfo  applicationInfo)
    {
        _jellyPlayerApiService = jellyPlayerApiService;
        _configurationService = configurationService;
        _playerService = playerService;
        _fileService = fileService;
        ApplicationInfo = applicationInfo;
    }

    public void Dispose()
    {
    }
}