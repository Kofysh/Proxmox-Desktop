using System.Collections.ObjectModel;
using System.Net.Http;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProxmoxDesktop.Api;
using ProxmoxDesktop.Api.Models;
using ProxmoxDesktop.Config;

namespace ProxmoxDesktop.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    // ─── Server ──────────────────────────────────────────────────────────────────────────
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string server = string.Empty;

    [ObservableProperty] private string port    = "8006";
    [ObservableProperty] private bool   skipSsl;

    // ─── Credentials ────────────────────────────────────────────────────────────────────
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string username = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string password = string.Empty;

    [ObservableProperty] private string  otp         = string.Empty;
    [ObservableProperty] private bool    totpVisible;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private RealmData? selectedRealm;

    public ObservableCollection<RealmData> Realms { get; } = [];

    // ─── API Token ─────────────────────────────────────────────────────────────────────
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private bool useApiToken;

    partial void OnUseApiTokenChanged(bool value) => ErrorMessage = null;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string tokenId = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string tokenSecret = string.Empty;

    // ─── State ──────────────────────────────────────────────────────────────────────────
    [ObservableProperty] private bool    isBusy;
    [ObservableProperty] private string? errorMessage;

    private readonly ConfigurationService _config;
    private ApiClient? _api;
    public  IApiClient? ApiClient => _api;

    public LoginViewModel(ConfigurationService config)
    {
        _config = config;
        LoadSaved();
    }

    // ─── Commands ─────────────────────────────────────────────────────────────────────

    [RelayCommand]
    public async Task LoadRealmsAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(Server) || string.IsNullOrWhiteSpace(Port)) return;
        IsBusy = true; ErrorMessage = null; Realms.Clear();
        try
        {
            _api?.Dispose();
            _api = new ApiClient(NormalizeServer(Server), NormalizePort(Port), SkipSsl);
            var version = await _api.VerifyProxmoxAsync(ct);
            foreach (var r in await _api.GetRealmsAsync(ct)) Realms.Add(r);
            SelectedRealm = Realms.FirstOrDefault();
            if (Realms.Count == 0)
                throw new InvalidOperationException($"Proxmox VE {version} was detected, but no authentication realms were returned.");
        }
        catch (UriFormatException) { ErrorMessage = "Enter a valid server hostname or IP address."; _api = null; }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            // A reverse proxy can protect the public version endpoint with HTTP auth.
            // Keep the API client alive and let the Proxmox login endpoint validate credentials.
            Realms.Add(new RealmData { Realm = "pam", Type = "pam", Comment = "Default Proxmox realm" });
            SelectedRealm = Realms[0];
        }
        catch (HttpRequestException ex) { ErrorMessage = $"This address is not reachable as Proxmox VE: {ex.Message}"; _api = null; }
        catch (FormatException) { ErrorMessage = "Port must be a number between 1 and 65535."; _api = null; }
        catch (Exception ex)
        {
            ErrorMessage = $"This address is not a Proxmox VE server: {ex.Message}";
            _api = null;
        }
        finally { IsBusy = false; }
    }

    [RelayCommand(CanExecute = nameof(CanLogin))]
    public async Task LoginAsync(CancellationToken ct = default)
    {
        IsBusy = true; ErrorMessage = null;
        try
        {
            // Always validate the current endpoint before sending credentials.
            await LoadRealmsAsync(ct);
            if (_api is null) return;
            var result = UseApiToken
                ? await _api.LoginWithTokenAsync(TokenId.Trim(), TokenSecret.Trim(), ct)
                : await _api.LoginAsync(
                    Username.Trim(), Password,
                    SelectedRealm?.Realm ?? "pam",
                    TotpVisible ? Otp : null, ct);

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
        return UseApiToken
            ? !string.IsNullOrWhiteSpace(TokenId) && !string.IsNullOrWhiteSpace(TokenSecret)
            : !string.IsNullOrWhiteSpace(Username)
              && !string.IsNullOrWhiteSpace(Password)
              && SelectedRealm is not null;
    }

    public event Action<IApiClient>? OnLoginSuccess;

    // ─── Persistence ───────────────────────────────────────────────────────────────────

    private void LoadSaved()
    {
        var c       = _config.Config;
        Server      = c.Server;
        Port        = c.Port;
        Username    = c.Username;
        SkipSsl     = c.SkipSsl;
        UseApiToken = c.UseApiToken;
        TokenId     = c.TokenId;
    }

    private void SaveCredentials()
    {
        var c           = _config.Config;
        c.Server        = Server;
        c.Port          = Port;
        c.SkipSsl       = SkipSsl;
        c.UseApiToken   = UseApiToken;
        if (UseApiToken) c.TokenId  = TokenId;
        else             c.Username = Username;
        _config.Save();
    }

    private static string NormalizeServer(string value)
    {
        var input = value.Trim();
        if (!input.Contains("://", StringComparison.Ordinal))
            return input;
        return new Uri(input, UriKind.Absolute).Host;
    }

    private static string NormalizePort(string value)
    {
        if (!int.TryParse(value.Trim(), out var parsed) || parsed is < 1 or > 65535)
            throw new FormatException();
        return parsed.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }
}
