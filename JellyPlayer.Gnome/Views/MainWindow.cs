using Gio;
using Gtk;
using JellyPlayer.Gnome.Helpers;
using JellyPlayer.Shared.Controls;
using JellyPlayer.Shared.Enums;
using JellyPlayer.Shared.Events;
using Task = System.Threading.Tasks.Task;

namespace JellyPlayer.Gnome.Views;

/// <summary>
/// The MainWindow for the application
/// </summary>
public partial class MainWindow : Adw.ApplicationWindow
{
    private readonly MainWindowController _controller;
    private readonly Adw.Application _application;
    
    private readonly StartupController _startupController;

    private readonly PlayerController _playerController;
    private readonly PlayerView  _playerView;
    
    private readonly AlbumController _albumController;
    private readonly AlbumView _albumView;
    
    private readonly AlbumListController _albumListController;
    private readonly AlbumListView _albumListView;
    
    private readonly SearchController _searchController;
    private readonly SearchView _searchView;
    
    private readonly QueueListController _queueListController;
    private readonly QueueListView _queueListView;
    
    private readonly PlaylistController _playlistController;
    private readonly PlaylistView _playlistView;

    [Gtk.Connect] private readonly Gtk.Button _searchButton;
    [Gtk.Connect] private readonly Gtk.SearchEntry _search_field;
    
    [Gtk.Connect] private readonly Gtk.Box _player;

    //[Gtk.Connect] private readonly Adw.ToastOverlay toastOverlay;
    
    [Gtk.Connect] private readonly Gtk.MenuButton _menuButton;
    [Gtk.Connect] private readonly Adw.Spinner _spinner;
    
    [Gtk.Connect] private readonly Adw.NavigationPage _main_view;
    [Gtk.Connect] private readonly Adw.NavigationPage _album_details;
    [Gtk.Connect] private readonly Adw.NavigationPage _search_albums;
    [Gtk.Connect] private readonly Adw.NavigationPage _queue_list;
    [Gtk.Connect] private readonly Adw.NavigationPage _playlist;
    
    [Gtk.Connect] private readonly Adw.NavigationView _album_view;
    [Gtk.Connect] private readonly Adw.ToolbarView _album_list_view;
    [Gtk.Connect] private readonly Adw.ToolbarView _album_details_view;
    [Gtk.Connect] private readonly Adw.ToolbarView _search_albums_view;
    [Gtk.Connect] private readonly Adw.ToolbarView _queue_list_view;
    [Gtk.Connect] private readonly Adw.ToolbarView _playlist_view;
    
    // This is stupid hack. Used for displaying shadow correctly on player
    [Gtk.Connect] private readonly Gtk.Box _main_view_footer;
    [Gtk.Connect] private readonly Gtk.Box _album_details_footer;
    [Gtk.Connect] private readonly Gtk.Box _search_albums_footer;
    [Gtk.Connect] private readonly Gtk.Box _queue_list_footer;
    [Gtk.Connect] private readonly Gtk.Box _playlist_footer;
    
    private MainWindow(Gtk.Builder builder, MainWindowController controller, Adw.Application application) : base(new Adw.Internal.ApplicationWindowHandle(builder.GetPointer("_root"), false))
    {
        //Window Settings
        _controller = controller;
        _application = application;
        SetTitle(_controller.ApplicationInfo.Name);
        SetIconName(_controller.ApplicationInfo.Icon);
        
        //Build UI
        builder.Connect(this);

        _controller.GetPlayerService().OnPlayerStateChanged += (sender, args) =>
        {
            if (args.State is PlayerState.Playing or PlayerState.Stopped or PlayerState.Paused)
            {
                if (!_main_view_footer.IsVisible())
                    _main_view_footer.SetVisible(true);
                
                if (!_album_details_footer.IsVisible())
                    _album_details_footer.SetVisible(true);
                
                if (!_player.IsVisible())
                    _player.SetVisible(true);
                
                if (!_search_albums_footer.IsVisible())
                    _search_albums_footer.SetVisible(true);
                
                if (!_queue_list_footer.IsVisible())
                    _queue_list_footer.SetVisible(true);
                
                if (!_playlist_footer.IsVisible())
                    _playlist_footer.SetVisible(true);
            }
            else if (args.State is PlayerState.None)
            {
                _player?.SetVisible(false);
                _main_view_footer?.SetVisible(false);
                _album_details_footer?.SetVisible(true);
                _search_albums_footer?.SetVisible(true);
                _queue_list_footer?.SetVisible(false);
                _playlist_footer?.SetVisible(false);
            }
        };
        
        // Album list
        _albumListController = new AlbumListController(_controller.GetJellyPlayerApiService(),
            _controller.GetConfigurationService(), _controller.GetPlayerService(), _controller.GetFileService());
        _albumListController.OnAlbumClicked += AlbumListControllerOnAlbumClicked;
        _albumListView = new AlbumListView(_albumListController);
        _album_list_view.SetContent(_albumListView);
        
        //Album details
        _albumController = new AlbumController(_controller.GetJellyPlayerApiService(), _controller.GetConfigurationService(), _controller.GetPlayerService(), _controller.GetFileService());
        _albumView = new AlbumView(_albumController);
        _album_details_view.SetContent(_albumView);
        
        // Startup
        _startupController = new StartupController(_controller.GetJellyPlayerApiService(), _controller.GetConfigurationService());

        //Audio player
        _playerController = new PlayerController(_controller.GetJellyPlayerApiService(), _controller.GetConfigurationService(), _controller.GetPlayerService());
        _playerController.OnShowPlaylistClicked += PlayerControllerOnShowPlaylistClicked;
        _playerController.OnShowShowLyricsClicked += PlayerControllerOnShowShowLyricsClicked;
        _playerView = new PlayerView(this, _playerController);
        _player.Append(_playerView);
        
        // Search
        _searchController = new SearchController(_controller.GetJellyPlayerApiService(), _controller.GetConfigurationService(), _controller.GetPlayerService(), _controller.GetFileService());
        _searchController.OnAlbumClicked += SearchControllerOnAlbumClicked;
        _searchView = new SearchView(_searchController);
        _search_albums_view.SetContent(_searchView);
        _search_field.OnSearchChanged += async (sender, args) =>
        {
            if (string.IsNullOrWhiteSpace(sender.GetText()))
            {
                _searchController.StartSearch();
                return;
            }
            
            _searchController.SearchAlbums(sender.GetText());
        };
        
        // Que list for currently playling queue
        _queueListController = new QueueListController(_controller.GetJellyPlayerApiService(), _controller.GetConfigurationService(), _controller.GetPlayerService(), _controller.GetFileService());
        _queueListView = new QueueListView(_queueListController);
        _queue_list_view.SetContent(_queueListView);
        
        // Playlist
        _playlistController = new PlaylistController(_controller.GetJellyPlayerApiService(), _controller.GetConfigurationService(), _controller.GetPlayerService(), _controller.GetFileService());
        _playlistView = new PlaylistView(_playlistController);
        _playlist.SetChild(_playlistView);
        
        var actPlaylist = Gio.SimpleAction.New("playlist", null);
        actPlaylist.OnActivate += ActPlaylistOnActivate;
        AddAction(actPlaylist);
        application.SetAccelsForAction("win.playlist", new string[] { "<Ctrl>p" });
        
        //Refresh application
        var actRefresh = Gio.SimpleAction.New("refresh", null);
        actRefresh.OnActivate += ActRefreshOnActivate;
        AddAction(actRefresh);
        application.SetAccelsForAction("win.refresh", new string[] { "<Ctrl>r" });

        //Preferences Action
        var actPreferences = Gio.SimpleAction.New("preferences", null);
        actPreferences.OnActivate += ActPreferencesOnOnActivate;
        AddAction(actPreferences);
        application.SetAccelsForAction("win.preferences", new string[] { "<Ctrl>comma" });
        
        //About Action
        var actAbout = Gio.SimpleAction.New("about", null);
        actAbout.OnActivate += ActAboutOnOnActivate;
        AddAction(actAbout);

        // Set search visible/hidden
        var actSearchBar = Gio.SimpleAction.New("search", null);
        actSearchBar.OnActivate += ActShowSearchBarOnOnActivate;
        AddAction(actSearchBar);
        application.SetAccelsForAction("win.search", new string[] { "<Ctrl>f" });

        //Quit Action
        var actQuit = Gio.SimpleAction.New("quit", null);
        actQuit.OnActivate += Quit;
        AddAction(actQuit);
        application.SetAccelsForAction("win.quit", new string[] { "<Ctrl>q" });
        
        var ctrlEvent = Gtk.EventControllerKey.New();
        ctrlEvent.OnKeyPressed += (sender, args) =>
        {
            _albumView.IsCtrlActive(true);
            return true;
        };

        ctrlEvent.OnKeyReleased += (sender, args) =>
        {
            _albumView.IsCtrlActive(false);
        };
        
        AddController(ctrlEvent);
    }

    private void ActPlaylistOnActivate(SimpleAction sender, SimpleAction.ActivateSignalArgs args)
    {
        var visiblePageName = _album_view.GetVisiblePage()?.Tag;
        _album_view.Pop();
        
        if (visiblePageName == "_playlist") return;

        _playlistController.RefreshPlaylist();
        _album_view.Push(_playlist);
    }

    private void PlayerControllerOnShowShowLyricsClicked(object? sender, AlbumArgs e)
    {
        var controller = new LyricsController(_controller.GetJellyPlayerApiService(), _controller.GetPlayerService());
        var lyrics = new LyricsView(controller);
        lyrics.Present(this);
        controller.Update();
    }

    private void PlayerControllerOnShowPlaylistClicked(object? sender, AlbumArgs e)
    {
        var visiblePageName = _album_view.GetVisiblePage()?.Tag;
        _album_view.Pop();
        
        if (visiblePageName == "_queue_list") return;
        
        _queueListController.Open();
        _album_view.Push(_queue_list);
    }

    private void SearchControllerOnAlbumClicked(object? sender, AlbumArgs args)
    {
        var visiblePageName = _album_view.GetVisiblePage()?.Tag;
        if (visiblePageName != "_search_albums")
            _album_view.Pop();
        
        _albumController.Open(args.AlbumId, args.TrackId);
        _album_view.Push(_album_details);
    }

    private void AlbumListControllerOnAlbumClicked(object? sender, Guid albumId)
    {
        _albumController.Open(albumId);
        _album_view.Push(_album_details);
    }

    private void ActRefreshOnActivate(Gio.SimpleAction sender, Gio.SimpleAction.ActivateSignalArgs args)
    {
        _albumListController.RefreshAlbums(true);
    }

    private void ActAboutOnOnActivate(Gio.SimpleAction sender, Gio.SimpleAction.ActivateSignalArgs args)
    {
        var about = Adw.AboutDialog.New();
        about.ApplicationName = _controller.ApplicationInfo.Name;
        about.ApplicationIcon = _controller.ApplicationInfo.Icon;
        about.DeveloperName = _controller.ApplicationInfo.Developer;
        about.Version = _controller.ApplicationInfo.Version;
        about.Website = _controller.ApplicationInfo.Website;
        about.Copyright = _controller.ApplicationInfo.Copyright;
        about.IssueUrl = _controller.ApplicationInfo.IssueUrl;
        about.ReleaseNotes = _controller.ApplicationInfo.ReleaseNotes;
        about.LicenseType = License.Gpl30;
        about.Designers = _controller.ApplicationInfo.Designers;
        about.Artists = _controller.ApplicationInfo.Artists;
        about.Present(this);
    }

    /// <summary>
    /// Show preferences dialog
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void ActPreferencesOnOnActivate(Gio.SimpleAction sender, Gio.SimpleAction.ActivateSignalArgs args)
    {
        var preferences = new PreferencesView(_controller.GetConfigurationService(), _controller.GetJellyPlayerApiService());
        preferences.Present(this);
        preferences.OnClosed += async (dialog, eventArgs) =>
        {
            if (preferences.Refresh)
            {
                await _startupController.StartAsync(preferences.Password);
                _albumListController.RefreshAlbums(true);
            }
        };
    }
    
    private void ActShowSearchBarOnOnActivate(Gio.SimpleAction sender, Gio.SimpleAction.ActivateSignalArgs args)
    {
        var visiblePageName = _album_view.GetVisiblePage()?.Tag;
        if (visiblePageName != "_search_albums")
            _album_view.Pop();
        
        _album_view.Push(_search_albums);
        _search_field.SetText(string.Empty);
        _search_field.GrabFocus();
        _searchController.StartSearch();
    }

    /// <summary>
    /// Constructs a MainWindow
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="controller">The MainWindowController</param>
    /// <param name="application">The Adw.Application</param>
    public MainWindow(Adw.Application sender, MainWindowController controller, Adw.Application application) : this(Blueprint.BuilderFromFile("window"), controller, application)
    {
    }

    /// <summary>
    /// Starts the MainWindow
    /// </summary>
    public async Task StartAsync()
    {
        _application.AddWindow(this);
        Present();

        var startupState = await _startupController.StartAsync();
        if (startupState == StartupState.RequirePassword)
        {
            var taskCompletionSource = new TaskCompletionSource();
            _spinner.SetVisible(false);
            var login = new LoginView(_startupController, taskCompletionSource);
            login.Present(this);
            await taskCompletionSource.Task;
        }
        else if (startupState != StartupState.Finished)
        {
            var taskCompletionSource = new TaskCompletionSource();
            _spinner.SetVisible(false);
            var startup = new StartupView(_startupController, taskCompletionSource);
            startup.Present(this);
            await taskCompletionSource.Task;
        }

        _spinner.SetVisible(false);
        _album_view.SetVisible(true);
        _albumListController.RefreshAlbums();
    }

    public override void Dispose()
    {
        _albumController.Dispose();
        _playerController.Dispose();
        _albumController.Dispose();
        _searchController.Dispose();
        _playlistController.Dispose();
        _startupController.Dispose();
        _queueListController.Dispose();
        base.Dispose();
    }

    /// <summary>
    /// Occurs when quit action is triggered
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void Quit(Gio.SimpleAction sender, EventArgs e) => _application.Quit();
}
