using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ProxmoxDesktop.Core.Api.Internal;
using ProxmoxDesktop.Core.Api.Models;

namespace ProxmoxDesktop.Core.Api;

/// <summary>
/// Client HTTP asynchrone pour l'API Proxmox VE.
/// Toutes les méthodes sont async/await — aucun .GetAwaiter().GetResult().
/// Utilise System.Text.Json (pas de Newtonsoft.Json).
/// Le mot de passe n'est jamais conservé après l'authentification.
/// </summary>
public sealed class ApiClient : IDisposable
{
    // -------------------------------------------------------------------------
    // Options JSON partagées
    // -------------------------------------------------------------------------
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    // -------------------------------------------------------------------------
    // Champs privés
    // -------------------------------------------------------------------------
    private readonly HttpClient _http;
    private readonly string     _baseUrl;
    private DataTicket?         _ticket;
    private bool                _disposed;

    // -------------------------------------------------------------------------
    // Constructeur
    // -------------------------------------------------------------------------

    /// <param name="server">IP ou hostname du serveur Proxmox (sans https://).</param>
    /// <param name="port">Port de l'API PVE, généralement 8006.</param>
    /// <param name="skipSsl">Si true, accepte les certificats self-signed sans erreur.</param>
    public ApiClient(string server, string port, bool skipSsl)
    {
        _baseUrl = $"https://{server}:{port}/api2/json/";

        var handler = new HttpClientHandler { UseCookies = false };
        if (skipSsl)
            handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;

        _http = new HttpClient(handler)
        {
            BaseAddress = new Uri(_baseUrl),
            Timeout     = TimeSpan.FromSeconds(15)
        };
        _http.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    // =========================================================================
    // AUTHENTIFICATION
    // =========================================================================

    /// <summary>
    /// Récupère la liste des realms disponibles sur le serveur.
    /// Appelé avant le login pour peupler le ComboBox de la LoginPage.
    /// </summary>
    public async Task<List<RealmData>> GetRealmsAsync(CancellationToken ct = default)
    {
        var response = await _http.GetAsync("access/domains", ct).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        var parsed = JsonSerializer.Deserialize<PveListResponse<RealmData>>(body, _jsonOptions);
        return parsed?.Data ?? [];
    }

    /// <summary>
    /// Tente l'authentification username/password (+TOTP optionnel).
    /// Le mot de passe n'est pas conservé après cet appel.
    /// </summary>
    public async Task<LoginResult> LoginAsync(
        string username, string password, string realm,
        string? otp = null, CancellationToken ct = default)
    {
        var form = new Dictionary<string, string>
        {
            ["username"] = $"{username}@{realm}",
            ["password"] = password,
            ["realm"]    = realm
        };
        if (!string.IsNullOrWhiteSpace(otp))
            form["otp"] = otp;

        var result = await PostTicketAsync(form, ct).ConfigureAwait(false);
        if (result is null) return LoginResult.Failure("Réponse invalide du serveur.");

        // Ticket PVE indique que le TFA est requis
        if (result.Ticket?.Contains("PVE:!tfa!") == true)
        {
            if (string.IsNullOrWhiteSpace(otp))
                return LoginResult.TotpRequired();

            // Deuxième passe : challenge TOTP
            return await TotpChallengeAsync(result.Ticket, result.Username ?? username, otp, ct)
                .ConfigureAwait(false);
        }

        // Succès direct
        if (result.Ticket is not null && result.CsrfPreventionToken is not null)
        {
            _ticket = new DataTicket
            {
                Ticket              = result.Ticket,
                CsrfPreventionToken = result.CsrfPreventionToken,
                Username            = result.Username ?? username
            };
            return LoginResult.Success();
        }

        return LoginResult.Failure("Identifiants incorrects.");
    }

    private async Task<LoginResult> TotpChallengeAsync(
        string tfaTicket, string username, string otp, CancellationToken ct)
    {
        var form = new Dictionary<string, string>
        {
            ["username"]      = username,
            ["password"]      = $"totp:{otp}",
            ["tfa-challenge"] = tfaTicket
        };

        var result = await PostTicketAsync(form, ct).ConfigureAwait(false);
        if (result?.Ticket is null || result.CsrfPreventionToken is null)
            return LoginResult.Failure("Code TOTP invalide.");

        _ticket = new DataTicket
        {
            Ticket              = result.Ticket,
            CsrfPreventionToken = result.CsrfPreventionToken,
            Username            = result.Username ?? username
        };
        return LoginResult.Success();
    }

    /// <summary>Renouvelle le ticket d'auth existant (appel toutes les ~90 min).</summary>
    public async Task RenewTicketAsync(CancellationToken ct = default)
    {
        EnsureAuthenticated();
        var form = new Dictionary<string, string>
        {
            ["username"] = _ticket!.Username,
            ["password"] = _ticket.Ticket   // Le ticket courant sert de password
        };
        var result = await PostTicketAsync(form, ct).ConfigureAwait(false);
        if (result?.Ticket is not null && result.CsrfPreventionToken is not null)
        {
            _ticket = new DataTicket
            {
                Ticket              = result.Ticket,
                CsrfPreventionToken = result.CsrfPreventionToken,
                Username            = result.Username ?? _ticket.Username
            };
        }
    }

    // =========================================================================
    // NŒUDS & MACHINES
    // =========================================================================

    /// <summary>Retourne la liste des nœuds du cluster.</summary>
    public async Task<List<NodeData>> GetNodesAsync(CancellationToken ct = default)
    {
        var json = await GetAsync("nodes", ct).ConfigureAwait(false);
        var parsed = JsonSerializer.Deserialize<PveListResponse<NodeData>>(json, _jsonOptions);
        return parsed?.Data ?? [];
    }

    /// <summary>
    /// Retourne toutes les VMs QEMU + containers LXC de tous les nœuds,
    /// en parallèle (une requête par nœud pour chaque type).
    /// </summary>
    public async Task<List<MachineData>> GetAllMachinesAsync(CancellationToken ct = default)
    {
        var nodes = await GetNodesAsync(ct).ConfigureAwait(false);

        // Lancer toutes les requêtes en parallèle
        var tasks = nodes.SelectMany(n => new[]
        {
            FetchMachinesForNodeAsync(n.Node, "qemu", ct),
            FetchMachinesForNodeAsync(n.Node, "lxc",  ct)
        });

        var results = await Task.WhenAll(tasks).ConfigureAwait(false);
        return results.SelectMany(r => r).ToList();
    }

    private async Task<List<MachineData>> FetchMachinesForNodeAsync(
        string node, string type, CancellationToken ct)
    {
        try
        {
            var json = await GetAsync($"nodes/{node}/{type}", ct).ConfigureAwait(false);
            var parsed = JsonSerializer.Deserialize<PveListResponse<MachineData>>(json, _jsonOptions);
            if (parsed?.Data is null) return [];

            // Injecter le nodename et le type (absents de la réponse list)
            return parsed.Data.Select(m =>
                m with { NodeName = node, Type = type }).ToList();
        }
        catch { return []; }  // Un nœud hors-ligne ne doit pas bloquer les autres
    }

    // =========================================================================
    // ACTIONS D'ALIMENTATION
    // =========================================================================

    /// <summary>
    /// Exécute une action d'alimentation sur une VM/CT.
    /// <paramref name="action"/> : start | shutdown | reboot | stop | reset | suspend.
    /// <paramref name="hibernate"/> : true pour suspendre en mode hibernation (QEMU uniquement).
    /// </summary>
    public async Task<PowerResult> PowerActionAsync(
        MachineData machine, string action,
        bool hibernate = false, CancellationToken ct = default)
    {
        var type = machine.IsLxc ? "lxc" : "qemu";
        var path = $"nodes/{machine.NodeName}/{type}/{machine.Vmid}/status/{action}";

        var postData = new Dictionary<string, string>();
        if (hibernate && action == "suspend")
            postData["todisk"] = "1";

        var raw = await PostAsync(path, postData, ct).ConfigureAwait(false);
        return raw switch
        {
            null    => PowerResult.Error,
            "403"   => PowerResult.Forbidden,
            _       => PowerResult.Ok
        };
    }

    // =========================================================================
    // CONSOLES
    // =========================================================================

    /// <summary>
    /// Génère l'URL d'accès à une console NoVNC ou xTermJS.
    /// <paramref name="consoleType"/> : "novnc" ou "xtermjs".
    /// </summary>
    public async Task<string?> GetConsoleUrlAsync(
        MachineData machine, string consoleType, CancellationToken ct = default)
    {
        EnsureAuthenticated();
        var type   = machine.IsLxc ? "lxc" : "qemu";
        var server = _http.BaseAddress!.Host;
        var port   = _http.BaseAddress.Port;

        // Générer le VNC ticket pour cette VM
        var path     = $"nodes/{machine.NodeName}/{type}/{machine.Vmid}/vncproxy";
        var postData = new Dictionary<string, string> { ["websocket"] = "1" };
        var raw      = await PostAsync(path, postData, ct).ConfigureAwait(false);
        if (raw is null or "403") return null;

        var resp = JsonSerializer.Deserialize<PveResponse<VncTicketData>>(raw, _jsonOptions);
        if (resp?.Data is null) return null;

        var vncTicket = Uri.EscapeDataString(resp.Data.Ticket ?? string.Empty);
        var vmName    = Uri.EscapeDataString(machine.Name);
        var authTicket= Uri.EscapeDataString(_ticket!.Ticket);

        return consoleType == "xtermjs"
            ? $"https://{server}:{port}/?console=xtermjs&vmid={machine.Vmid}&vmname={vmName}"
              + $"&node={machine.NodeName}&resize=1&cmd="
              + $"#pve_data=token_ticket={vncTicket}"
            : $"https://{server}:{port}/?console=kvm&novnc=1&vmid={machine.Vmid}&vmname={vmName}"
              + $"&node={machine.NodeName}&resize=scale&autoconnect=1"
              + $"&path=api2/json/nodes/{machine.NodeName}/{type}/{machine.Vmid}/vncwebsocket"
              + $"&vncticket={vncTicket}&PVEAuthCookie={authTicket}";
    }

    /// <summary>Récupère la configuration SPICE pour une VM (QEMU uniquement).</summary>
    public async Task<SpiceObject?> GetSpiceConfigAsync(
        MachineData machine, string? proxy = null, CancellationToken ct = default)
    {
        if (machine.IsLxc) return null;

        var path     = $"nodes/{machine.NodeName}/qemu/{machine.Vmid}/spiceproxy";
        var postData = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(proxy))
            postData["proxy"] = proxy!;

        var raw = await PostAsync(path, postData, ct).ConfigureAwait(false);
        if (raw is null or "403") return null;

        var resp = JsonSerializer.Deserialize<PveResponse<SpiceObject>>(raw, _jsonOptions);
        return resp?.Data;
    }

    // =========================================================================
    // HELPERS HTTP INTERNES
    // =========================================================================

    private async Task<string> GetAsync(string path, CancellationToken ct = default)
    {
        EnsureAuthenticated();
        using var req = new HttpRequestMessage(HttpMethod.Get, path);
        AddAuthHeaders(req);
        var resp = await _http.SendAsync(req, ct).ConfigureAwait(false);
        if (resp.StatusCode == HttpStatusCode.Forbidden) return "403";
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
    }

    private async Task<string?> PostAsync(
        string path, Dictionary<string, string>? data, CancellationToken ct = default)
    {
        EnsureAuthenticated();
        using var req = new HttpRequestMessage(HttpMethod.Post, path);
        AddAuthHeaders(req, includeCsrf: true);
        if (data is { Count: > 0 })
            req.Content = new FormUrlEncodedContent(data);

        var resp = await _http.SendAsync(req, ct).ConfigureAwait(false);
        if (resp.StatusCode == HttpStatusCode.Forbidden) return "403";
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
    }

    /// <summary>
    /// POST sur /access/ticket (sans auth cookie — utilisé pour login et renew).
    /// </summary>
    private async Task<TicketResponse?> PostTicketAsync(
        Dictionary<string, string> form, CancellationToken ct)
    {
        using var content = new FormUrlEncodedContent(form);
        var resp  = await _http.PostAsync("access/ticket", content, ct).ConfigureAwait(false);
        if (!resp.IsSuccessStatusCode) return null;
        var body   = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        var parsed = JsonSerializer.Deserialize<PveResponse<TicketResponse>>(body, _jsonOptions);
        return parsed?.Data;
    }

    private void AddAuthHeaders(HttpRequestMessage req, bool includeCsrf = false)
    {
        req.Headers.TryAddWithoutValidation("Cookie", $"PVEAuthCookie={_ticket!.Ticket}");
        if (includeCsrf)
            req.Headers.TryAddWithoutValidation("CSRFPreventionToken", _ticket.CsrfPreventionToken);
    }

    private void EnsureAuthenticated()
    {
        if (_ticket is null)
            throw new InvalidOperationException("ApiClient : non authentifié. Appelez LoginAsync() d'abord.");
    }

    // =========================================================================
    // IDisposable
    // =========================================================================

    public void Dispose()
    {
        if (_disposed) return;
        _http.Dispose();
        _disposed = true;
    }

    // -------------------------------------------------------------------------
    // DTO interne VNC ticket
    // -------------------------------------------------------------------------
    private sealed class VncTicketData
    {
        [System.Text.Json.Serialization.JsonPropertyName("ticket")]
        public string? Ticket { get; init; }
        [System.Text.Json.Serialization.JsonPropertyName("port")]
        public int Port { get; init; }
    }
}
