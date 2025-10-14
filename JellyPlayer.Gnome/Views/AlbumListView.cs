using Gtk.Internal;
using JellyPlayer.Gnome.Helpers;
using JellyPlayer.Gnome.Models;
using JellyPlayer.Shared.Controls;
using JellyPlayer.Shared.Models;
using SignalListItemFactory = Gtk.SignalListItemFactory;

namespace JellyPlayer.Gnome.Views;

public class AlbumListView : Gtk.ScrolledWindow
{
    private readonly AlbumListController  _controller;
    
    [Gtk.Connect] private readonly Adw.Spinner _spinner;
    [Gtk.Connect] private readonly Gtk.Box _results;
    
    [Gtk.Connect] private readonly Gtk.ListView _albumList;
    [Gtk.Connect] private readonly Gtk.ScrolledWindow _albumListWindow;
    private readonly Gtk.SignalListItemFactory _albumListFactory;
    
    [Gtk.Connect] private readonly Gtk.GridView _albumGrid;
    [Gtk.Connect] private readonly Gtk.ScrolledWindow _albumGridWindow;
    private readonly Gtk.SignalListItemFactory _albumGridFactory;
    
    private readonly Gio.ListStore _albumListItems;

    private AlbumListView(Gtk.Builder builder) : base(
        new ScrolledWindowHandle(builder.GetPointer("_root"), false))
    {
        builder.Connect(this);
    }
    
    public AlbumListView(AlbumListController controller) : this(Blueprint.BuilderFromFile("album_list"))
    {
        _controller = controller;
        _controller.OnAlbumListChanged += async (sender, args) =>
        {
            if (args.Albums is not null)
            {
                _albumListItems.RemoveAll();
                foreach (var album in args.Albums)
                {
                    _albumListItems.Append(new AlbumRow(album));   
                }
            }
            
            if (args.IsLoading)
            {
                _results.SetVisible(false);
                _spinner.SetVisible(true);
            }
            else
            {
                _spinner.SetVisible(false);
                _results.SetVisible(true);
            }
        };

        var configuration = _controller.GetConfigurationService().Get();
        _albumList.SetShowSeparators(configuration.ShowListSeparator);
        
        _controller.GetConfigurationService().Saved += (sender, args) =>
        {
            var updatedConfiguration = _controller.GetConfigurationService().Get();
            _albumList.SetShowSeparators(updatedConfiguration.ShowListSeparator);
        };
        
        _albumListItems = Gio.ListStore.New(AlbumRow.GetGType());
        var selectionModel = Gtk.NoSelection.New(_albumListItems);
        
        //Album list
        _albumListFactory = Gtk.SignalListItemFactory.New();
        _albumListFactory.OnSetup += AlbumListFactoryOnSetup;
        _albumListFactory.OnBind += AlbumListFactoryOnBind;
        _albumListFactory.OnUnbind += AlbumListFactoryOnUnbind;
        _albumList.SetFactory(_albumListFactory);
        _albumList.SetModel(selectionModel);
        _albumList.OnActivate += async (sender, args) =>
        {
            var row = _albumListItems.GetObject(args.Position) as AlbumRow;
            if (row != null)
            {
                _controller.OpenAlbum(row.Id);
            }
        };
        
        // Album grid
        _albumGridFactory = Gtk.SignalListItemFactory.New();
        _albumGridFactory.OnSetup += AlbumGridFactoryOnSetup;
        _albumGridFactory.OnBind += AlbumGridFactoryOnBind;
        _albumGridFactory.OnUnbind += AlbumGridFactoryOnUnbind;
        _albumGrid.SetFactory(_albumGridFactory);
        _albumGrid.SetModel(selectionModel);
        _albumGrid.OnActivate += async (sender, args) =>
        {
            var row = _albumListItems.GetObject(args.Position) as AlbumRow;
            if (row != null)
            {
                _controller.OpenAlbum(row.Id);
            }
        };
    }

    private uint? FindIndexForAlbumId(Guid albumId)
    {
        for (uint index = 0; index < _albumListItems.GetNItems(); index++)
        {
            if (_albumListItems.GetObject(index) is AlbumRow row && row.Id == albumId)
                return index;
        }

        return null;
    }

    private void AlbumGridFactoryOnUnbind(SignalListItemFactory sender, SignalListItemFactory.UnbindSignalArgs args)
    {
        var listItem = args.Object as Gtk.ListItem;
        if (listItem is null)
        {
            return;
        }
        
        var template = listItem.Child as AlbumGridItem;
        if (template is null)
        {
            return;
        }

        template.Clear();
    }

    private void AlbumListFactoryOnUnbind(SignalListItemFactory sender, SignalListItemFactory.UnbindSignalArgs args)
    {
        var listItem = args.Object as Gtk.ListItem;
        if (listItem is null)
        {
            return;
        }

        var template = listItem.Child as AlbumListItem;
        if (template is null)
        {
            return;
        }

        template.Clear();
    }

    private void AlbumGridFactoryOnBind(Gtk.SignalListItemFactory sender, Gtk.SignalListItemFactory.BindSignalArgs args)
    {
        var listItem = args.Object as Gtk.ListItem;
        if (listItem is null)
        {
            return;
        }
        
        var template = listItem.Child as AlbumGridItem;
        if (template is null)
        {
            return;
        }

        if (listItem.Item is AlbumRow item)
            template.Bind(item);
    }

    private void AlbumGridFactoryOnSetup(Gtk.SignalListItemFactory sender, Gtk.SignalListItemFactory.SetupSignalArgs args)
    {
        var listItem = args.Object as Gtk.ListItem;
        if (listItem is null)
        {
            return;
        }
        
        listItem.SetChild(new AlbumGridItem(_controller.GetFileService()));
    }

    private void AlbumListFactoryOnBind(Gtk.SignalListItemFactory sender, Gtk.SignalListItemFactory.BindSignalArgs args)
    {
        var listItem = args.Object as Gtk.ListItem;
        if (listItem is null)
        {
            return;
        }

        var template = listItem.Child as AlbumListItem;
        if (template is null)
        {
            return;
        }

        if (listItem.Item is AlbumRow item)
            template.Bind(item);
    }

    private void AlbumListFactoryOnSetup(Gtk.SignalListItemFactory sender, Gtk.SignalListItemFactory.SetupSignalArgs args)
    {
        var listItem = args.Object as Gtk.ListItem;
        if (listItem is null)
        {
            return;
        }

        listItem.SetChild(new AlbumListItem(_controller.GetFileService()));
    }
}