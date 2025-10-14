using System;
using System.Threading.Tasks;
using Adw.Internal;
using JellyPlayer.Gnome.Helpers;
using JellyPlayer.Gnome.Models;
using JellyPlayer.Shared.Controls;
using SwitchRow = Adw.SwitchRow;

namespace JellyPlayer.Gnome.Views;

public class AccountView : Adw.PreferencesGroup
{
    private readonly AccountController  _controller;

    [Gtk.Connect] private readonly Adw.EntryRow _server;
    [Gtk.Connect] private readonly Adw.EntryRow _username;
    [Gtk.Connect] private readonly Adw.PasswordEntryRow _password;
    [Gtk.Connect] private readonly Adw.SwitchRow _rememberPassword;
    [Gtk.Connect] private readonly Adw.ComboRow _collection;

    private readonly Gtk.SignalListItemFactory _collectionFactory;
    private readonly Gio.ListStore _collectionItems;
    
    private Adw.Spinner _serverLoading = Adw.Spinner.New();
    private Adw.Spinner _usernameLoading = Adw.Spinner.New();
    private Adw.Spinner _passwordLoading = Adw.Spinner.New();
    private Adw.Spinner _collectionLoading = Adw.Spinner.New();
    
    private bool _isServerValid;
    private bool _isAccountValid;
    private bool _isCollectionValid;
    
    private AccountView(Gtk.Builder builder) : base(
        new PreferencesGroupHandle(builder.GetPointer("_root"), false))
    {
        builder.Connect(this);
    }
    
    public AccountView(AccountController controller) : this(Blueprint.BuilderFromFile("account"))
    {
        _controller = controller;
        _controller.OnConfigurationLoaded += async (sender, configuration) =>
        {
            _isAccountValid = false;
            _isServerValid = false;
            
            _server.SetText(configuration.ServerUrl);
            _username.SetText(configuration.Username);
            _password.SetText(configuration.Password);
            _rememberPassword.SetActive(configuration.RememberPassword);

            await CheckServer();
            await CheckLogin();
            
            _controller.UpdateValidity(_isServerValid,  _isAccountValid, _isCollectionValid);
        };

        _serverLoading.SetVisible(false);
        _server.AddSuffix(_serverLoading);
        _server.OnApply += async (sender, args) =>
        {
            await CheckServer();
        };

        _usernameLoading.SetVisible(false);
        _username.AddSuffix(_usernameLoading);
        _username.OnApply += async (sender, args) =>
        {
            _usernameLoading.SetVisible(true);
           await CheckLogin();
        };

        _passwordLoading.SetVisible(false);
        _password.AddSuffix(_passwordLoading);
        _password.OnApply += async (sender, args) =>
        {
            _passwordLoading.SetVisible(true);
            await CheckLogin();
        };

        _rememberPassword.OnNotify += (sender, args) =>
        {
            if (sender is SwitchRow element)
                _controller.RememberPassword = element.GetActive();
        };
        
        _collectionItems = Gio.ListStore.New(CollectionRow.GetGType());
        var selectionModel = Gtk.NoSelection.New(_collectionItems);
        _collectionFactory = Gtk.SignalListItemFactory.New();
        _collectionFactory.OnBind += CollectionFactoryOnBind;
        _collectionFactory.OnSetup += CollectionFactoryOnSetup;
        _collection.SetFactory(_collectionFactory);
        _collection.SetModel(selectionModel);
        
        _collectionLoading.SetVisible(false);
        _collection.AddSuffix(_collectionLoading);
    }

    private void CollectionFactoryOnSetup(Gtk.SignalListItemFactory sender, Gtk.SignalListItemFactory.SetupSignalArgs args)
    {
        var listItem = args.Object as Gtk.ListItem;
        if (listItem is null)
        {
            return;
        }

        var label = Gtk.Label.New(null);
        listItem.SetChild(label);
    }

    private void CollectionFactoryOnBind(Gtk.SignalListItemFactory sender, Gtk.SignalListItemFactory.BindSignalArgs args)
    {
        var listItem = args.Object as Gtk.ListItem;
        if (listItem is null)
        {
            return;
        }

        var template = listItem.Child as Gtk.Label;
        if (template is null)
        {
            return;
        }

        if (listItem.Item is CollectionRow item)
            template.SetText(item.Name);
    }

    private async Task CheckServer()
    {
        _server.RemoveCssClass("error");
        _username.SetSensitive(false);
        _password.SetSensitive(false);
        _collection.SetSensitive(false);
            
        if (!string.IsNullOrWhiteSpace(_server.GetText()))
        {
            _serverLoading.SetVisible(true);
            var serverUrl = _server.GetText();
            _isServerValid = await _controller.IsValidServer(serverUrl);
            _serverLoading.SetVisible(false);

            if (_isServerValid)
            {
                _controller.ServerUrl = serverUrl;
                _controller.UpdateValidity(true, false, false);
                await CheckLogin();
                _username.SetSensitive(true);
                _password.SetSensitive(true);
            }
            else
            {
                _server.AddCssClass("error");
            }
        }
        else
        {
            _server.AddCssClass("error");
        }
    }
    
    private async Task CheckLogin()
    {
        var username = _username.GetText().Trim();
        var password = _password.GetText().Trim();
        
        if (!_isServerValid)
        {
            _username.RemoveCssClass("error");
            _password.RemoveCssClass("error");
            _usernameLoading.SetVisible(false);
            _passwordLoading.SetSensitive(false);
        }
        
        if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
        {
            _collection.SetSensitive(false);
            _isAccountValid = await _controller.IsValidAccount(username, password);
            _usernameLoading.SetVisible(false);
            _passwordLoading.SetVisible(false);
            
            if (_isAccountValid)
            {
                _controller.Username = username;
                _controller.Password = password;
                _controller.UpdateValidity(true,  true, false);
                _username.RemoveCssClass("error");
                _password.RemoveCssClass("error");
                _collection.SetSensitive(true);
                await UpdateCollections();
            }
            else
            {
                _username.AddCssClass("error");
                _password.AddCssClass("error");
            }
        }
        else
        {
            _usernameLoading.SetVisible(false);
            _passwordLoading.SetVisible(false);
        }
    }

    private async Task UpdateCollections()
    {
        _collection.RemoveCssClass("error");
        _isCollectionValid = false;
        
        if (_isServerValid && _isAccountValid)
        {
            _collectionLoading.SetVisible(true);
            _collectionItems.RemoveAll();
            
            var selectedIndex = -1;
            var collectionId = _controller.GetSelectedCollectionId();
            var collections = await _controller.GetCollections();
            
            for (var index = 0; index < collections.Count; index++)
            {
                var collection = collections[index];
                if (collection.Id == collectionId)
                    selectedIndex = index;
                
                _collectionItems.Append(new CollectionRow(collection));
            }

            if (selectedIndex != -1)
            {
                _collection.SetSelected(Convert.ToUInt32(selectedIndex));
                _controller.CollectionId = collectionId;
                _controller.UpdateValidity(true, true, true);
                _isCollectionValid = true;
            }
            else if (collections.Count > 0)
            {
                _collection.SetSelected(0);
                _controller.CollectionId = (_collection.GetSelectedItem() as CollectionRow)?.Id;
                _controller.UpdateValidity(true, true, true);
                _isCollectionValid = true;
            }
            else
            {
                _collection.AddCssClass("error");
                _controller.UpdateValidity(true, true, false);
                _isCollectionValid = false;
            }
            
            _controller.UpdateValidity(_isServerValid,  _isAccountValid, _isCollectionValid);
            _collection.SetSensitive(true);
            _collectionLoading.SetVisible(false);
        }
    }
}