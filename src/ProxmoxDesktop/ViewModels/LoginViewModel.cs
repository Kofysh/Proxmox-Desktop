using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProxmoxDesktop.Api;
using ProxmoxDesktop.Api.Models;
using ProxmoxDesktop.Config;

namespace ProxmoxDesktop.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    [ObservableProperty] public partial string     Server        { get; set; } = string.Empty;
    [ObservableProperty] public partial string     Port          { get; set; } = "8006";
    [ObservableProperty] public partial string     Username      { get; set; } = string.Empty;
    [ObservableProperty] public partial string     Password      { get; set; } = string.Empty;
    [ObservableProperty] public partial string     Otp           { get; set; } = string.Empty;
    [ObservableProperty] public partial bool       SkipSsl       { get; set; }
    [ObservableProperty] public partial bool       IsBusy        { get; set; }
    [ObservableProperty] public partial bool       TotpVisible   { get; set; }
    [ObservableProperty] public partial string?    ErrorMessage  { get; set; }
    [ObservableProperty] public partial RealmData? SelectedRealm { get; set; }

    public ObservableCollection<RealmData> Realms { get; } = [];

    private readonly ConfigurationService _config;
    private ApiClient? _api;
    public  ApiClient? ApiClient => _api;

    public LoginViewModel(ConfigurationService config)
    {
        _config = config;
        LoadSaved();
    }

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
        if (_api is null) return;
        IsBusy = true; ErrorMessage = null;
        try
        {
            var result = await _api.LoginAsync(
                Username.Trim(), Password,
                SelectedRealm?.Realm ?? "pam",
                TotpVisible ? Otp : null);

            if      (result.IsSuccess) { SaveCredentials(); OnLoginSuccess?.Invoke(_api); }
            else if (result.NeedsTotp) TotpVisible = true;
            else                       ErrorMessage = result.Message;
        }
        catch (Exception ex) { ErrorMessage = $"Login error: {ex.Message}"; }
        finally { IsBusy = false; }
    }

    private bool CanLogin() =>
        !string.IsNullOrWhiteSpace(Username) &&
        !string.IsNullOrWhiteSpace(Password) &&
        SelectedRealm is not null && !IsBusy;

    public event Action<ApiClient>? OnLoginSuccess;

    private void LoadSaved()
    {
        Server   = _config.Get<string>("server")   ?? string.Empty;
        Port     = _config.Get<string>("port")     ?? "8006";
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
    }
}
