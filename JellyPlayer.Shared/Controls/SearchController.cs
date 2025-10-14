using JellyPlayer.Shared.Events;
using JellyPlayer.Shared.Models;
using JellyPlayer.Shared.Services;

namespace JellyPlayer.Shared.Controls;

public class SearchController
{
    private readonly IJellyPlayerApiService _jellyPlayerApiService;
    private readonly IConfigurationService _configurationService;
    private readonly IPlayerService _playerService;
    private readonly IFileService  _fileService;
    
    public readonly List<Models.Search> Results = [];
    public IFileService GetFileService() => _fileService;
    public event EventHandler<AlbumArgs> OnAlbumClicked;
    public event EventHandler<SearchStateArgs>? OnSearchStateChanged;
    
    public SearchController(IJellyPlayerApiService jellyPlayerApiService, IConfigurationService configurationService, IPlayerService playerService, IFileService fileService)
    {
        _jellyPlayerApiService = jellyPlayerApiService;
        _configurationService = configurationService;
        _playerService = playerService;
        _fileService = fileService;
    }

    /// <summary>
    /// Show search startup page
    /// </summary>
    public void StartSearch()
    {
        SearchStateChanged(new SearchStateArgs() { Open = true });
    }
    
    /// <summary>
    /// Open album with id
    /// </summary>
    /// <param name="albumId"></param>
    public void OpenAlbum(Guid albumId, Guid? trackId)
    {
        OnAlbumClicked.Invoke(this, new AlbumArgs() { AlbumId = albumId, TrackId = trackId });
    }
    
    private protected virtual void SearchStateChanged(SearchStateArgs e)
    {
        OnSearchStateChanged?.Invoke(this, e);
    }
    
    /// <summary>
    /// Begin searching for value
    /// </summary>
    /// <param name="value"></param>
    public async Task SearchAlbums(string value)
    {
        SearchStateChanged(new SearchStateArgs() { Start = true });
        Results.Clear();
        
        var results = await Task.WhenAll([
            _jellyPlayerApiService.SearchAlbum(value),
            _jellyPlayerApiService.SearchArtistAlbums(value),
            _jellyPlayerApiService.SearchTrack(value),
        ]);
        
        var sortList = new List<Search>();
        foreach (var result in results)
        {
            sortList.AddRange(result);
        }
        
        // Removes duplicates and sorts
        var sorted = sortList.GroupBy(x => x.Id).Select(x => x.First()).OrderBy(s => s.Type);
        Results.AddRange(sorted);
        
        SearchStateChanged(new SearchStateArgs() { Updated = true });
    }
}