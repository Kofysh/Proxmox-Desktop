using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using ProxmoxDesktop.Core.Api.Models;

namespace ProxmoxDesktop.Core.Api;

/// <summary>
/// Client HTTP async pour l'API Proxmox VE.
/// Toutes les méthodes sont async — aucun appel bloquant.
/// </summary>
public class ApiClient : IDisposable
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public DataTicket? Ticket { get; private set; }
    public ServerInfo Server { get; }
    public List<MachineData> Machines { get; private set; } = [];

    public ApiClient(string server, string port, bool skipSsl)
    {
        Server = new ServerInfo(server, port, skipSsl);
        var handler = new HttpClientHandler { UseCookies = false };
        if (skipSsl)
            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

        _http = new HttpClient(handler)
        {
            BaseAddress = new Uri($"https://{server}:{port}/api2/json/"),
            Timeout = TimeSpan.FromSeconds(15)
        };
    }

    // -------------------------------------------------------------------------
    // Auth
    // -------------------------------------------------------------------------

    public async Task<List<RealmData>> GetRealmsAsync(CancellationToken ct = default)
    {
        var json = await GetRawAsync("access/domains", authenticated: false, ct);
        var root = JsonSerializer.Deserialize<RootObject<RealmData>>(json, _jsonOptions);
        return root?.Data ?? [];
    }

    public async Task<LoginResult> LoginAsync(string username, string password, string realm, string? otp = null, CancellationToken ct = default)
    {
        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["username"] = username,
            ["password"] = password,
            ["realm"]    = realm
        });

        using var response = await _http.PostAsync("access/ticket", form, ct);
        if (!response.IsSuccessStatusCode)
            return LoginResult.Failure("Identifiants incorrects.");

        var json = await response.Content.ReadAsStringAsync(ct);
        var loginResp = JsonSerializer.Deserialize<LoginResponse>(json, _jsonOptions);
        Ticket = loginResp?.Data;

        if (Ticket is null) return LoginResult.Failure("Réponse invalide du serveur.");

        if (Ticket.RequiresTotp)
        {
            if (string.IsNullOrWhiteSpace(otp))
                return LoginResult.TotpRequired();
            return await TotpChallengeAsync(otp, ct);
        }

        return LoginResult.Success();
    }

    private async Task<LoginResult> TotpChallengeAsync(string otpCode, CancellationToken ct)
    {
        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["username"]      = Ticket!.Username,
            ["password"]      = $"totp:{otpCode}",
            ["tfa-challenge"] = Ticket.Ticket
        });

        using var response = await _http.PostAsync("access/ticket", form, ct);
        if (!response.IsSuccessStatusCode)
            return LoginResult.Failure("Code TOTP invalide.");

        var json = await response.Content.ReadAsStringAsync(ct);
        var loginResp = JsonSerializer.Deserialize<LoginResponse>(json, _jsonOptions);
        Ticket = loginResp?.Data;
        return Ticket is not null ? LoginResult.Success() : LoginResult.Failure("Erreur TOTP.");
    }

    public async Task RenewTicketAsync(CancellationToken ct = default)
    {
        if (Ticket is null) return;
        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["username"] = Ticket.Username,
            ["password"] = Ticket.Ticket   // Le ticket actuel sert de password pour le renouvellement
        });
        using var response = await _http.PostAsync("access/ticket", form, ct);
        if (!response.IsSuccessStatusCode) return;
        var json = await response.Content.ReadAsStringAsync(ct);
        var loginResp = JsonSerializer.Deserialize<LoginResponse>(json, _jsonOptions);
        if (loginResp?.Data is not null) Ticket = loginResp.Data;
    }

    // -------------------------------------------------------------------------
    // Machines
    // -------------------------------------------------------------------------

    public async Task<List<MachineData>> GetAllMachinesAsync(CancellationToken ct = default)
    {
        var all = new List<MachineData>();
        var nodesJson = await GetRawAsync("nodes", ct: ct);
        var nodesRoot = JsonSerializer.Deserialize<RootObject<NodeData>>(nodesJson, _jsonOptions);
        if (nodesRoot?.Data is null) return all;

        var tasks = nodesRoot.Data.SelectMany(node => new[]
        {
            FetchMachinesForNodeAsync(node, "lxc",  ct),
            FetchMachinesForNodeAsync(node, "qemu", ct)
        });

        var results = await Task.WhenAll(tasks);
        all.AddRange(results.SelectMany(r => r));
        Machines = [.. all.OrderBy(m => m.Vmid)];
        return Machines;
    }

    private async Task<List<MachineData>> FetchMachinesForNodeAsync(NodeData node, string type, CancellationToken ct)
    {
        try
        {
            var json = await GetRawAsync($"nodes/{node.Node}/{type}", ct: ct);
            var root = JsonSerializer.Deserialize<RootObject<MachineData>>(json, _jsonOptions);
            if (root?.Data is null) return [];
            foreach (var m in root.Data) m.NodeName = node.Node;
            return root.Data;
        }
        catch { return []; }
    }

    public async Task<bool> HasSpiceAsync(MachineData machine, CancellationToken ct = default)
    {
        try
        {
            var json = await GetRawAsync($"nodes/{machine.NodeName}/qemu/{machine.Vmid}/status/current", ct: ct);
            using var doc = JsonDocument.Parse(json);
            var spice = doc.RootElement.GetProperty("data").GetProperty("spice");
            return spice.ValueKind == JsonValueKind.Number && spice.GetInt32() == 1;
        }
        catch { return false; }
    }

    // -------------------------------------------------------------------------
    // Power
    // -------------------------------------------------------------------------

    public async Task<PowerResult> PowerActionAsync(MachineData machine, string action, bool toDisk = false, CancellationToken ct = default)
    {
        var data = new Dictionary<string, string>
        {
            ["node"]  = machine.NodeName,
            ["vmid"]  = machine.Vmid.ToString()
        };
        if (toDisk && action == "suspend") data["todisk"] = "1";

        var result = await PostRawAsync(
            $"nodes/{machine.NodeName}/{machine.Type}/{machine.Vmid}/status/{action}",
            data, ct);

        return result switch
        {
            "403" => PowerResult.Forbidden,
            null  => PowerResult.Error,
            _     => PowerResult.Success
        };
    }

    // -------------------------------------------------------------------------
    // Console (NoVNC / xTermJS)
    // -------------------------------------------------------------------------

    public async Task<string?> GetConsoleUrlAsync(MachineData machine, string consoleType, CancellationToken ct = default)
    {
        string path = machine.IsLxc
            ? $"nodes/{machine.NodeName}/lxc/{machine.Vmid}/vncwebsocket"
            : $"nodes/{machine.NodeName}/qemu/{machine.Vmid}/vncwebsocket";

        // On reconstruit l'URL complète de la WebUI Proxmox pour le console WebView2
        string ticketEncoded = Uri.EscapeDataString(Ticket!.Ticket);
        string baseUrl = $"https://{Server.Host}:{Server.Port}";
        string consoleUrl = consoleType switch
        {
            "novnc"   => $"{baseUrl}/?console=kvm&novnc=1&vmid={machine.Vmid}&vmname={machine.Name}&node={machine.NodeName}&resize=off&PVEAuthCookie={ticketEncoded}",
            "xtermjs" => $"{baseUrl}/?console=shell&xtermjs=1&vmid={machine.Vmid}&vmname={machine.Name}&node={machine.NodeName}&PVEAuthCookie={ticketEncoded}",
            _         => $"{baseUrl}/?console=kvm&novnc=1&vmid={machine.Vmid}&vmname={machine.Name}&node={machine.NodeName}&PVEAuthCookie={ticketEncoded}"
        };
        return consoleUrl;
    }

    public async Task<SpiceObject?> GetSpiceConfigAsync(MachineData machine, string? proxyOverride = null, CancellationToken ct = default)
    {
        var data = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(proxyOverride)) data["proxy"] = proxyOverride;

        var json = await PostRawAsync(
            $"nodes/{machine.NodeName}/{machine.Type}/{machine.Vmid}/spiceproxy",
            data, ct);

        if (json is null || json == "403") return null;
        var resp = JsonSerializer.Deserialize<SpiceResponse>(json, _jsonOptions);
        return resp?.Data;
    }

    // -------------------------------------------------------------------------
    // HTTP primitives
    // -------------------------------------------------------------------------

    private async Task<string> GetRawAsync(string path, bool authenticated = true, CancellationToken ct = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, path);
        if (authenticated && Ticket is not null)
            req.Headers.TryAddWithoutValidation("Cookie", $"PVEAuthCookie={Ticket.Ticket}");
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await _http.SendAsync(req, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(ct);
    }

    private async Task<string?> PostRawAsync(string path, Dictionary<string, string> postData, CancellationToken ct = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = new FormUrlEncodedContent(postData)
        };
        if (Ticket is not null)
        {
            req.Headers.TryAddWithoutValidation("Cookie", $"PVEAuthCookie={Ticket.Ticket}");
            req.Headers.TryAddWithoutValidation("CSRFPreventionToken", Ticket.CsrfPreventionToken);
        }
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await _http.SendAsync(req, ct);
        if (response.StatusCode == HttpStatusCode.Forbidden) return "403";
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadAsStringAsync(ct);
    }

    public void Dispose() => _http.Dispose();
}
