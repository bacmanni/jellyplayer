using JellyPlayer.Shared.Models;
using JellyPlayer.Shared.Services;

namespace JellyPlayer.Shared.Controls;

public class QueueListController : IDisposable
{
    private readonly IJellyPlayerApiService _jellyPlayerApiService;
    private readonly IConfigurationService _configurationService;
    private readonly IPlayerService _playerService;
    private readonly IFileService _fileService;
    
    public readonly List<Models.Track> Tracks = [];
    
    public IFileService GetFileService() => _fileService;
    public IPlayerService GetPlayerService() => _playerService;
    
    public event EventHandler<QueueArgs> OnQueueUpdated;
    
    public QueueListController(IJellyPlayerApiService jellyPlayerApiService, IConfigurationService configurationService, IPlayerService playerService, IFileService fileService)
    {
        _jellyPlayerApiService = jellyPlayerApiService;
        _configurationService = configurationService;
        _playerService = playerService;
        _fileService = fileService;
    }

    /// <summary>
    /// Open current queue. Data is fetched from playerservice
    /// </summary>
    public void Open()
    {
        Tracks.Clear();
        var tracks = _playerService.GetTracks();
        tracks.Reverse();
        Tracks.AddRange(tracks);

        OnQueueUpdated.Invoke(this, new QueueArgs());
    }
    
    
    public void Dispose() 
    {

    }

    /// <summary>
    /// Randomize current queue
    /// </summary>
    public void ShuffleTracks()
    {
        var rnd = new Random();
        var tracks = Tracks.OrderBy(item => rnd.Next()).ToList();
        
        //Tracks.Clear();
        //Tracks.AddRange(tracks);
    }
    
    public void PlayTrack(Track track)
    {
        _playerService.PlayTrack(track);
    }

    public void RemoveTrack(Track track)
    {
        _playerService.RemoveTrack(track);
    }
}