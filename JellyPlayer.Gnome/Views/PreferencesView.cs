using Adw.Internal;
using JellyPlayer.Gnome.Helpers;
using JellyPlayer.Shared.Controls;
using JellyPlayer.Shared.Services;
using AlertDialog = Adw.AlertDialog;

namespace JellyPlayer.Gnome.Views;

public partial class PreferencesView : Adw.PreferencesDialog
{
    private readonly IConfigurationService _configurationService;
    private readonly IJellyPlayerApiService _jellyPlayerApiService;

    private readonly AccountController  _accountController;
    private readonly AccountView _accountView;
    
    [Gtk.Connect] private readonly Adw.PreferencesPage _preferencesPage1;

    [Gtk.Connect] private readonly Adw.SwitchRow _useLocalMemory;
    [Gtk.Connect] private readonly Adw.SwitchRow _showListSeparator;
    
    public bool Refresh { get; set; } = false;
    public string? Password { get; set; } = null;
    
    private PreferencesView(Gtk.Builder builder) : base(
        new PreferencesDialogHandle(builder.GetPointer("_root"), false))
    {
        builder.Connect(this);
        OnCloseAttempt += CloseAttempt;
    }

    private void CloseAttempt(Adw.Dialog sender, EventArgs args)
    {
        // We need to validate account so application won't break
        if (_accountController.IsValid())
        {
            var configuration = _configurationService.Get();
            configuration.CacheAlbumArt = _useLocalMemory.GetActive();
            configuration.ShowListSeparator = _showListSeparator.GetActive();
            
            Refresh = _accountController.HasChanges();
            configuration.ServerUrl = _accountController.ServerUrl;
            configuration.Username = _accountController.Username;

            // If password is not saved, we pass it temporarily through variable
            if (_accountController.RememberPassword)
            {
                configuration.Password = _accountController.Password;
            }
            else
            {
                configuration.Password = string.Empty;
                Password = _accountController.Password;
            }
            
            configuration.RememberPassword = _accountController.RememberPassword;
            configuration.CollectionId = _accountController.CollectionId?.ToString() ?? throw new NullReferenceException("This should never happen!");
            configuration.PlaylistCollectionId = _accountController.PlaylistCollectionId?.ToString();
            
            _configurationService.Set(configuration);
            _configurationService.Save();
            ForceClose();
        }
        else
        {
            var alert = new PreferencesAlert();
            alert.Present(this);
            alert.OnResponse += AlertOnResponse;
        }
    }

    private void AlertOnResponse(AlertDialog sender, AlertDialog.ResponseSignalArgs args)
    {
        if (args.Response == "close")
            ForceClose();
    }

    public PreferencesView(IConfigurationService configurationService, IJellyPlayerApiService jellyPlayerApiService) : this(Blueprint.BuilderFromFile("preferences"))
    {
        _configurationService = configurationService;
        _jellyPlayerApiService = jellyPlayerApiService;
        
        _accountController = new AccountController(_configurationService, _jellyPlayerApiService);
        _accountView =  new AccountView(_accountController);
        _preferencesPage1.Add(_accountView);
        
        var configuration =  _configurationService.Get();
        _accountController.OpenConfiguration(configuration);
        _useLocalMemory.SetActive(configuration.CacheAlbumArt);
        _showListSeparator.SetActive(configuration.ShowListSeparator);
    }

    public override void Dispose()
    {
        OnCloseAttempt -= CloseAttempt;
        base.Dispose();
    }
}