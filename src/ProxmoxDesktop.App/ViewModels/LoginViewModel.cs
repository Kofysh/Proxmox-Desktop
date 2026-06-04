using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProxmoxDesktop.Core.Api;
using ProxmoxDesktop.Core.Api.Models;
using ProxmoxDesktop.Core.Config;

namespace ProxmoxDesktop.App.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    // -------------------------------------------------------------------------
    // State
    // -------------------------------------------------------------------------

    [ObservableProperty]
    private string _server = string.Empty;

    [ObservableProperty]
    private string _port = "8006";

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _otp = string.Empty;

    [ObservableProperty]
    private bool _skipSsl;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _totpVisible;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private RealmData? _selectedRealm;

    public ObservableCollection<RealmData> Realms { get; } = [];

    // -------------------------------------------------------------------------
    // Dependencies
    // -------------------------------------------------------------------------

    private readonly ConfigurationService _config;
    private ApiClient? _api;

    public ApiClient? ApiClient => _api;

    public LoginViewModel(ConfigurationService config)
    {
        _config = config;
        LoadSavedCredentials();
    }

    // -------------------------------------------------------------------------
    // Commands
    // -------------------------------------------------------------------------

    /// <summary>Charge les realms disponibles quand l'URL du serveur est validée.</summary>
    [RelayCommand]
    public async Task LoadRealmsAsync()
    {
        if (string.IsNullOrWhiteSpace(Server) || string.IsNullOrWhiteSpace(Port)) return;

        IsBusy = true;
        ErrorMessage = null;
        Realms.Clear();

        try
        {
            _api = new ApiClient(Server.Trim(), Port.Trim(), SkipSsl);
            var realms = await _api.GetRealmsAsync();
            foreach (var r in realms) Realms.Add(r);
            SelectedRealm = Realms.FirstOrDefault();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Impossible de contacter le serveur : {ex.Message}";
            _api = null;
        }
        finally { IsBusy = false; }
    }

    /// <summary>Tente l'authentification avec les identifiants fournis.</summary>
    [RelayCommand(CanExecute = nameof(CanLogin))]
    public async Task LoginAsync()
    {
        if (_api is null) return;

        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var result = await _api.LoginAsync(
                Username.Trim(),
                Password,
                SelectedRealm?.Realm ?? "pam",
                TotpVisible ? Otp : null);

            if (result.IsSuccess)
            {
                SaveCredentials();
                OnLoginSuccess?.Invoke();
            }
            else if (result.NeedsTotp)
            {
                TotpVisible = true;
            }
            else
            {
                ErrorMessage = result.Message;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur lors de la connexion : {ex.Message}";
        }
        finally { IsBusy = false; }
    }

    private bool CanLogin() =>
        !string.IsNullOrWhiteSpace(Username) &&
        !string.IsNullOrWhiteSpace(Password) &&
        SelectedRealm is not null &&
        !IsBusy;

    // -------------------------------------------------------------------------
    // Events
    // -------------------------------------------------------------------------

    /// <summary>Déclenché quand le login réussit — la View s'en charge pour naviguer.</summary>
    public event Action? OnLoginSuccess;

    // -------------------------------------------------------------------------
    // Persistence
    // -------------------------------------------------------------------------

    private void LoadSavedCredentials()
    {
        Server   = _config.Get<string>("server") ?? string.Empty;
        Port     = _config.Get<string>("port")   ?? "8006";
        Username = _config.Get<string>("username") ?? string.Empty;
        SkipSsl  = _config.Get<bool>("skipSsl");
    }

    private void SaveCredentials()
    {
        _config.Set("server",   Server);
        _config.Set("port",     Port);
        _config.Set("username", Username);
        _config.Set("skipSsl",  SkipSsl);
        _config.Save();
        // Le mot de passe n'est jamais persisté
    }
}
