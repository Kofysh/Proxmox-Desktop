using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProxmoxDesktop.Api;
using ProxmoxDesktop.Api.Models;
using ProxmoxDesktop.Config;

namespace ProxmoxDesktop.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    // -------------------------------------------------------------------------
    // Shared
    // -------------------------------------------------------------------------
    [ObservableProperty] public partial string  Server        { get; set; } = string.Empty;
    [ObservableProperty] public partial string  Port          { get; set; } = "8006";
    [ObservableProperty] public partial bool    SkipSsl       { get; set; }
    [ObservableProperty] public partial bool    IsBusy        { get; set; }
    [ObservableProperty] public partial string? ErrorMessage  { get; set; }

    // -------------------------------------------------------------------------
    // Auth mode toggle
    // -------------------------------------------------------------------------
    [ObservableProperty] public partial bool UseApiToken { get; set; }

    partial void OnUseApiTokenChanged(bool _) { ErrorMessage = null; }

    // -------------------------------------------------------------------------
    // Password auth fields
    // -------------------------------------------------------------------------
    [ObservableProperty] public partial string     Username      { get; set; } = string.Empty;
    [ObservableProperty] public partial string     Password      { get; set; } = string.Empty;
    [ObservableProperty] public partial string     Otp           { get; set; } = string.Empty;
    [ObservableProperty] public partial bool       TotpVisible   { get; set; }
    [ObservableProperty] public partial RealmData? SelectedRealm { get; set; }
    public ObservableCollection<RealmData> Realms { get; } = [];

    // -------------------------------------------------------------------------
    // API Token auth fields
    // -------------------------------------------------------------------------
    /// <summary>Token ID in format user@realm!tokenid, e.g. root@pam!mytoken</summary>
    [ObservableProperty] public partial string TokenId     { get; set; } = string.Empty;
    /// <summary>Token secret UUID, e.g. xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx</summary>
    [ObservableProperty] public partial string TokenSecret { get; set; } = string.Empty;

    // -------------------------------------------------------------------------
    // Dependencies
    // -------------------------------------------------------------------------
    private readonly ConfigurationService _config;
    private ApiClient? _api;
    public  ApiClient? ApiClient => _api;

    public LoginViewModel(ConfigurationService config)
    {
        _config = config;
        LoadSaved();
    }

    // -------------------------------------------------------------------------
    // Commands
    // -------------------------------------------------------------------------

    [RelayCommand]
    public async Task LoadRealmsAsync()
    {
        if (string.IsNullOrWhiteSpace(Server) || string.IsNullOrWhiteSpace(Port)) return;
        IsBusy = true; ErrorMessage = null; Realms.Clear();
        try
        {
            _api = new ApiClient(Server.Trim(), Port.Trim(), SkipSsl);
            foreach (var r in await _api.GetRealmsAsync()) Realms.Add(r);
            SelectedRealm = Realms.FirstOrDefault();
        }
        catch (Exception ex) { ErrorMessage = $"Cannot reach server: {ex.Message}"; _api = null; }
        finally { IsBusy = false; }
    }

    [RelayCommand(CanExecute = nameof(CanLogin))]
    public async Task LoginAsync()
    {
        // Ensure ApiClient is created (may not be if user skipped LostFocus on server field)
        _api ??= new ApiClient(Server.Trim(), Port.Trim(), SkipSsl);
        IsBusy = true; ErrorMessage = null;
        try
        {
            LoginResult result;
            if (UseApiToken)
            {
                result = await _api.LoginWithTokenAsync(TokenId.Trim(), TokenSecret.Trim());
            }
            else
            {
                result = await _api.LoginAsync(
                    Username.Trim(), Password,
                    SelectedRealm?.Realm ?? "pam",
                    TotpVisible ? Otp : null);
            }

            if      (result.IsSuccess) { SaveCredentials(); OnLoginSuccess?.Invoke(_api); }
            else if (result.NeedsTotp) TotpVisible = true;
            else                       ErrorMessage = result.Message;
        }
        catch (Exception ex) { ErrorMessage = $"Login error: {ex.Message}"; }
        finally { IsBusy = false; }
    }

    private bool CanLogin()
    {
        if (IsBusy || string.IsNullOrWhiteSpace(Server)) return false;
        if (UseApiToken)
            return !string.IsNullOrWhiteSpace(TokenId) && !string.IsNullOrWhiteSpace(TokenSecret);
        return !string.IsNullOrWhiteSpace(Username) &&
               !string.IsNullOrWhiteSpace(Password) &&
               SelectedRealm is not null;
    }

    public event Action<ApiClient>? OnLoginSuccess;

    // -------------------------------------------------------------------------
    // Persistence
    // -------------------------------------------------------------------------

    private void LoadSaved()
    {
        Server       = _config.Get<string>("server")      ?? string.Empty;
        Port         = _config.Get<string>("port")        ?? "8006";
        Username     = _config.Get<string>("username")    ?? string.Empty;
        SkipSsl      = _config.Get<bool>("skipSsl");
        UseApiToken  = _config.Get<bool>("useApiToken");
        TokenId      = _config.Get<string>("tokenId")     ?? string.Empty;
        // TokenSecret is never persisted
    }

    private void SaveCredentials()
    {
        _config.Set("server",       Server);
        _config.Set("port",         Port);
        _config.Set("skipSsl",      SkipSsl);
        _config.Set("useApiToken",  UseApiToken);
        if (UseApiToken)
        {
            _config.Set("tokenId", TokenId);
            // Secret is NEVER saved
        }
        else
        {
            _config.Set("username", Username);
        }
        _config.Save();
    }
}
