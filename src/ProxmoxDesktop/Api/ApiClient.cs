using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ProxmoxDesktop.Api.Internal;
using ProxmoxDesktop.Api.Models;

namespace ProxmoxDesktop.Api;

public sealed class ApiClient : IDisposable
{
    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _http;
    private readonly string     _baseUrl;
    private DataTicket?         _ticket;
    private bool                _disposed;

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
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    // =========================================================================
    // AUTH
    // =========================================================================

    public async Task<List<RealmData>> GetRealmsAsync(CancellationToken ct = default)
    {
        var r = await _http.GetAsync("access/domains", ct).ConfigureAwait(false);
        r.EnsureSuccessStatusCode();
        var body = await r.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        return JsonSerializer.Deserialize<PveListResponse<RealmData>>(body, _json)?.Data ?? [];
    }

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
        if (!string.IsNullOrWhiteSpace(otp)) form["otp"] = otp;

        var result = await PostTicketAsync(form, ct).ConfigureAwait(false);
        if (result is null) return LoginResult.Failure("Invalid server response.");

        if (result.Ticket?.Contains("PVE:!tfa!") == true)
        {
            if (string.IsNullOrWhiteSpace(otp)) return LoginResult.TotpRequired();
            return await TotpChallengeAsync(result.Ticket, result.Username ?? username, otp, ct);
        }

        if (result.Ticket is not null && result.CsrfPreventionToken is not null)
        {
            _ticket = new DataTicket { Ticket = result.Ticket, CsrfPreventionToken = result.CsrfPreventionToken, Username = result.Username ?? username };
            return LoginResult.Success();
        }
        return LoginResult.Failure("Invalid credentials.");
    }

    private async Task<LoginResult> TotpChallengeAsync(string tfaTicket, string username, string otp, CancellationToken ct)
    {
        var form = new Dictionary<string, string>
        {
            ["username"]      = username,
            ["password"]      = $"totp:{otp}",
            ["tfa-challenge"] = tfaTicket
        };
        var result = await PostTicketAsync(form, ct).ConfigureAwait(false);
        if (result?.Ticket is null || result.CsrfPreventionToken is null)
            return LoginResult.Failure("Invalid TOTP code.");
        _ticket = new DataTicket { Ticket = result.Ticket, CsrfPreventionToken = result.CsrfPreventionToken, Username = result.Username ?? username };
        return LoginResult.Success();
    }

    public async Task RenewTicketAsync(CancellationToken ct = default)
    {
        EnsureAuthenticated();
        var form = new Dictionary<string, string>
        {
            ["username"] = _ticket!.Username,
            ["password"] = _ticket.Ticket
        };
        var result = await PostTicketAsync(form, ct).ConfigureAwait(false);
        if (result?.Ticket is not null && result.CsrfPreventionToken is not null)
            _ticket = new DataTicket { Ticket = result.Ticket, CsrfPreventionToken = result.CsrfPreventionToken, Username = result.Username ?? _ticket.Username };
    }

    // =========================================================================
    // NODES & MACHINES
    // =========================================================================

    public async Task<List<NodeData>> GetNodesAsync(CancellationToken ct = default)
    {
        var json = await GetAsync("nodes", ct).ConfigureAwait(false);
        return JsonSerializer.Deserialize<PveListResponse<NodeData>>(json, _json)?.Data ?? [];
    }

    public async Task<List<MachineData>> GetAllMachinesAsync(CancellationToken ct = default)
    {
        var nodes = await GetNodesAsync(ct).ConfigureAwait(false);
        var tasks = nodes.SelectMany(n => new[]
        {
            FetchMachinesForNodeAsync(n.Node, "qemu", ct),
            FetchMachinesForNodeAsync(n.Node, "lxc",  ct)
        });
        var results = await Task.WhenAll(tasks).ConfigureAwait(false);
        return results.SelectMany(r => r).ToList();
    }

    private async Task<List<MachineData>> FetchMachinesForNodeAsync(string node, string type, CancellationToken ct)
    {
        try
        {
            var json   = await GetAsync($"nodes/{node}/{type}", ct).ConfigureAwait(false);
            var parsed = JsonSerializer.Deserialize<PveListResponse<MachineData>>(json, _json);
            return parsed?.Data?.Select(m => m with { NodeName = node, Type = type }).ToList() ?? [];
        }
        catch { return []; }
    }

    // =========================================================================
    // POWER
    // =========================================================================

    public async Task<PowerResult> PowerActionAsync(
        MachineData machine, string action, bool hibernate = false, CancellationToken ct = default)
    {
        var type = machine.IsLxc ? "lxc" : "qemu";
        var path = $"nodes/{machine.NodeName}/{type}/{machine.Vmid}/status/{action}";
        var data = new Dictionary<string, string>();
        if (hibernate && action == "suspend") data["todisk"] = "1";
        var raw = await PostAsync(path, data, ct).ConfigureAwait(false);
        return raw switch { null => PowerResult.Error, "403" => PowerResult.Forbidden, _ => PowerResult.Ok };
    }

    // =========================================================================
    // CONSOLE
    // =========================================================================

    public async Task<string?> GetConsoleUrlAsync(
        MachineData machine, string consoleType, CancellationToken ct = default)
    {
        EnsureAuthenticated();
        var type   = machine.IsLxc ? "lxc" : "qemu";
        var server = _http.BaseAddress!.Host;
        var port   = _http.BaseAddress.Port;
        var path   = $"nodes/{machine.NodeName}/{type}/{machine.Vmid}/vncproxy";
        var raw    = await PostAsync(path, new Dictionary<string, string> { ["websocket"] = "1" }, ct).ConfigureAwait(false);
        if (raw is null or "403") return null;
        var resp = JsonSerializer.Deserialize<PveResponse<VncTicketData>>(raw, _json);
        if (resp?.Data is null) return null;
        var vncTicket  = Uri.EscapeDataString(resp.Data.Ticket ?? string.Empty);
        var vmName     = Uri.EscapeDataString(machine.Name);
        var authTicket = Uri.EscapeDataString(_ticket!.Ticket);
        return consoleType == "xtermjs"
            ? $"https://{server}:{port}/?console=xtermjs&vmid={machine.Vmid}&vmname={vmName}&node={machine.NodeName}&resize=1&cmd=#pve_data=token_ticket={vncTicket}"
            : $"https://{server}:{port}/?console=kvm&novnc=1&vmid={machine.Vmid}&vmname={vmName}&node={machine.NodeName}&resize=scale&autoconnect=1&path=api2/json/nodes/{machine.NodeName}/{type}/{machine.Vmid}/vncwebsocket&vncticket={vncTicket}&PVEAuthCookie={authTicket}";
    }

    public async Task<SpiceObject?> GetSpiceConfigAsync(
        MachineData machine, string? proxy = null, CancellationToken ct = default)
    {
        if (machine.IsLxc) return null;
        var path = $"nodes/{machine.NodeName}/qemu/{machine.Vmid}/spiceproxy";
        var data = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(proxy)) data["proxy"] = proxy!;
        var raw = await PostAsync(path, data, ct).ConfigureAwait(false);
        if (raw is null or "403") return null;
        return JsonSerializer.Deserialize<PveResponse<SpiceObject>>(raw, _json)?.Data;
    }

    // =========================================================================
    // HTTP HELPERS
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

    private async Task<string?> PostAsync(string path, Dictionary<string, string>? data, CancellationToken ct = default)
    {
        EnsureAuthenticated();
        using var req = new HttpRequestMessage(HttpMethod.Post, path);
        AddAuthHeaders(req, includeCsrf: true);
        if (data is { Count: > 0 }) req.Content = new FormUrlEncodedContent(data);
        var resp = await _http.SendAsync(req, ct).ConfigureAwait(false);
        if (resp.StatusCode == HttpStatusCode.Forbidden) return "403";
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
    }

    private async Task<TicketResponse?> PostTicketAsync(Dictionary<string, string> form, CancellationToken ct)
    {
        using var content = new FormUrlEncodedContent(form);
        var resp  = await _http.PostAsync("access/ticket", content, ct).ConfigureAwait(false);
        if (!resp.IsSuccessStatusCode) return null;
        var body  = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        return JsonSerializer.Deserialize<PveResponse<TicketResponse>>(body, _json)?.Data;
    }

    private void AddAuthHeaders(HttpRequestMessage req, bool includeCsrf = false)
    {
        req.Headers.TryAddWithoutValidation("Cookie", $"PVEAuthCookie={_ticket!.Ticket}");
        if (includeCsrf) req.Headers.TryAddWithoutValidation("CSRFPreventionToken", _ticket.CsrfPreventionToken);
    }

    private void EnsureAuthenticated()
    {
        if (_ticket is null) throw new InvalidOperationException("Not authenticated. Call LoginAsync() first.");
    }

    public void Dispose()
    {
        if (_disposed) return;
        _http.Dispose();
        _disposed = true;
    }

    private sealed class VncTicketData
    {
        [System.Text.Json.Serialization.JsonPropertyName("ticket")] public string? Ticket { get; init; }
        [System.Text.Json.Serialization.JsonPropertyName("port")]   public int    Port   { get; init; }
    }
}
