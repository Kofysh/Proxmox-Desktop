using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProxmoxDesktop.Api;
using ProxmoxDesktop.Api.Models;
using ProxmoxDesktop.Services;

namespace ProxmoxDesktop.ViewModels;

public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly PeriodicTimer  _refreshTimer;
    private readonly PeriodicTimer  _ticketTimer;
    private CancellationTokenSource _cts = new();
    private Dictionary<string, string> _previousStatuses = [];

    /// <summary>One entry per connected Proxmox cluster.</summary>
    public ObservableCollection<ServerConnection> Connections { get; } = [];
    public ActivityLogService Activity { get; } = new();

    [ObservableProperty] private ObservableCollection<MachineData> machines         = [];
    [ObservableProperty] private ObservableCollection<NodeGroup>   groupedMachines  = [];
    [ObservableProperty] private ObservableCollection<MachineData> filteredMachines = [];
    [ObservableProperty] private ObservableCollection<string>      nodeList         = [];
    [ObservableProperty] private bool    isLoading;
    [ObservableProperty] private bool    isEmpty;
    [ObservableProperty] private string  searchQuery      = string.Empty;
    [ObservableProperty] private string? statusMessage;
    [ObservableProperty] private bool    isGroupedByNode;
    [ObservableProperty] private bool    isListView;
    [ObservableProperty] private bool    isActivityPanelOpen;
    [ObservableProperty] private int     sortIndex;        // 0=Name 1=VMID 2=CPU 3=RAM 4=Status 5=Uptime
    [ObservableProperty] private string? selectedNode;
    [ObservableProperty] private string? selectedTag;
    [ObservableProperty] private ServerConnection? selectedConnection;

    // Dashboard stats (cluster-wide, not affected by filters)
    [ObservableProperty] private int totalCount;
    [ObservableProperty] private int runningCount;
    [ObservableProperty] private int stoppedCount;
    [ObservableProperty] private int vmCount;
    [ObservableProperty] private int lxcCount;

    partial void OnSearchQueryChanged(string value)   => ApplyFilter();
    partial void OnIsGroupedByNodeChanged(bool value) => ApplyFilter();
    partial void OnIsListViewChanged(bool value)      => ApplyFilter();
    partial void OnSortIndexChanged(int value)        => ApplyFilter();
    partial void OnSelectedNodeChanged(string? value) => ApplyFilter();
    partial void OnSelectedTagChanged(string? value)  => ApplyFilter();
    partial void OnSelectedConnectionChanged(ServerConnection? value) { SelectedNode = null; ApplyFilter(); }

    public MainViewModel(ApiClient api, int refreshSeconds = 60)
    {
        Connections.Add(new ServerConnection(api.Host, api));
        var seconds = refreshSeconds < 10 ? 10 : refreshSeconds;
        _refreshTimer = new PeriodicTimer(TimeSpan.FromSeconds(seconds));
        _ticketTimer  = new PeriodicTimer(TimeSpan.FromMinutes(90));
        _ = Task.Run(async () => { while (await _refreshTimer.WaitForNextTickAsync(_cts.Token)) await RefreshAsync(_cts.Token); });
        _ = Task.Run(async () => { while (await _ticketTimer .WaitForNextTickAsync(_cts.Token)) await RenewAllTicketsAsync(_cts.Token); });
    }

    private async Task RenewAllTicketsAsync(CancellationToken ct)
    {
        foreach (var c in Connections.ToList())
            try { await c.Api.RenewTicketAsync(ct); } catch { /* surfaced on next call */ }
    }

    [RelayCommand]
    public async Task RefreshAsync(CancellationToken ct = default)
    {
        IsLoading = true; StatusMessage = null;
        try
        {
            var conns = Connections.ToList();
            var tasks = conns.Select(async c =>
            {
                try
                {
                    var ms = await c.Api.GetAllMachinesAsync(ct);
                    return ms.Select(m => m with { ServerName = c.Name }).ToList();
                }
                catch (OperationCanceledException) { throw; }
                catch (Exception ex) { Activity.Error($"{c.Name}: refresh failed", ex.Message); return new List<MachineData>(); }
            });
            var all = (await Task.WhenAll(tasks)).SelectMany(x => x).ToList();

            if (_previousStatuses.Count > 0)
                foreach (var m in all)
                {
                    var key = $"{m.ServerName}:{m.Vmid}";
                    if (_previousStatuses.TryGetValue(key, out var prev) && prev != m.Status)
                    {
                        NotificationService.NotifyStateChange(m.Name, m.Vmid, prev, m.Status);
                        Activity.Info($"{m.Name}: {prev} → {m.Status}", $"{m.ServerName} · {m.NodeName} · VMID {m.Vmid}");
                    }
                }
            _previousStatuses = all.ToDictionary(m => $"{m.ServerName}:{m.Vmid}", m => m.Status);

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                Machines.Clear();
                foreach (var m in all) Machines.Add(m);

                foreach (var c in Connections)
                    c.MachineCount = all.Count(m => m.ServerName == c.Name);

                TotalCount   = all.Count;
                RunningCount = all.Count(m => m.IsRunning);
                StoppedCount = all.Count(m => !m.IsRunning);
                VmCount      = all.Count(m => !m.IsLxc);
                LxcCount     = all.Count(m => m.IsLxc);
            });
            ApplyFilter();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { StatusMessage = $"Load error: {ex.Message}"; OnNotify?.Invoke(StatusMessage); }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    public async Task PowerActionAsync(PowerActionArgs args)
    {
        var api = ApiFor(args.Machine);
        if (api is null) return;

        var r = await api.PowerActionAsync(args.Machine, args.Action, args.Action == "suspend" && args.Extra);
        switch (r)
        {
            case PowerResult.Forbidden:
                StatusMessage = "Permission denied."; OnNotify?.Invoke(StatusMessage);
                Activity.Warning($"{args.Action}: {args.Machine.Name} — permission denied"); break;
            case PowerResult.Error:
                StatusMessage = "Power action failed."; OnNotify?.Invoke(StatusMessage);
                Activity.Error($"{args.Action}: {args.Machine.Name} — failed"); break;
            default:
                Activity.Success($"{args.Action} → {args.Machine.Name}", $"{args.Machine.ServerName} · {args.Machine.NodeName}"); break;
        }
        await Task.Delay(1500);
        await RefreshAsync();
    }

    // Convenience per-machine commands (used by the list/DataGrid view)
    [RelayCommand] public Task Start(MachineData m)     => PowerActionAsync(new PowerActionArgs(m, "start"));
    [RelayCommand] public Task Shutdown(MachineData m)  => PowerActionAsync(new PowerActionArgs(m, "shutdown"));
    [RelayCommand] public Task Reboot(MachineData m)    => PowerActionAsync(new PowerActionArgs(m, "reboot"));
    [RelayCommand] public Task ForceStop(MachineData m) => PowerActionAsync(new PowerActionArgs(m, "stop"));
    [RelayCommand] public Task OpenNoVnc(MachineData m) => OpenConsoleAsync(new ConsoleArgs(m, "novnc"));
    [RelayCommand] public Task OpenXterm(MachineData m) => OpenConsoleAsync(new ConsoleArgs(m, "xtermjs"));

    [RelayCommand]
    public async Task OpenConsoleAsync(ConsoleArgs args)
    {
        var api = ApiFor(args.Machine);
        if (api is null) return;
        var url = await api.GetConsoleUrlAsync(args.Machine, args.ConsoleType);
        if (url is not null) OnOpenConsole?.Invoke(args.Machine, url);
        else { OnNotify?.Invoke("Cannot open console."); Activity.Warning($"Console failed: {args.Machine.Name}"); }
    }

    [RelayCommand]
    public async Task OpenSpiceAsync(MachineData machine)
    {
        var api = ApiFor(machine);
        if (api is null) return;
        var cfg = await api.GetSpiceConfigAsync(machine);
        if (cfg is not null) OnOpenSpice?.Invoke(cfg);
        else OnNotify?.Invoke("SPICE is not available for this machine.");
    }

    [RelayCommand] public void Logout()            => OnLogout?.Invoke();
    [RelayCommand] public void ClearNodeFilter()   => SelectedNode       = null;
    [RelayCommand] public void ClearServerFilter() => SelectedConnection = null;
    [RelayCommand] public void ClearTagFilter()    => SelectedTag        = null;
    [RelayCommand] public void ClearSearch()       => SearchQuery        = string.Empty;
    [RelayCommand] public void FilterByTag(string tag)   => SelectedTag = tag;
    [RelayCommand] public void ToggleView()              => IsListView   = !IsListView;
    [RelayCommand] public void ToggleActivityPanel()     => IsActivityPanelOpen = !IsActivityPanelOpen;
    [RelayCommand] public void ClearActivity()           => Activity.Clear();
    [RelayCommand] public void AddServer()               => OnRequestAddServer?.Invoke();

    [RelayCommand]
    public async Task RemoveServer(ServerConnection? conn)
    {
        if (conn is null) return;
        if (Connections.Count <= 1) { OnNotify?.Invoke("At least one connection is required."); return; }
        Connections.Remove(conn);
        if (SelectedConnection == conn) SelectedConnection = null;
        Activity.Info($"Disconnected from {conn.Name}");
        try { conn.Api.Dispose(); } catch { }
        await RefreshAsync();
    }

    /// <summary>Adds an already-authenticated client as an additional cluster.</summary>
    public void AddConnection(IApiClient api)
    {
        var client = (ApiClient)api;
        if (Connections.Any(c => c.Name.Equals(client.Host, StringComparison.OrdinalIgnoreCase)))
        {
            OnNotify?.Invoke($"Already connected to {client.Host}.");
            try { client.Dispose(); } catch { }
            return;
        }
        Connections.Add(new ServerConnection(client.Host, client));
        Activity.Success($"Connected to {client.Host}");
        _ = RefreshAsync();
    }

    private ApiClient? ApiFor(MachineData m)
        => (Connections.FirstOrDefault(c => c.Name == m.ServerName) ?? Connections.FirstOrDefault())?.Api;

    private void ApplyFilter()
    {
        var q = SearchQuery.Trim();
        var source = Machines.AsEnumerable();

        if (SelectedConnection is not null)
            source = source.Where(m => m.ServerName == SelectedConnection.Name);

        // Node list reflects the currently selected server (or all servers).
        var nodes = source.Select(m => m.NodeName).Distinct().OrderBy(n => n).ToList();

        if (!string.IsNullOrEmpty(SelectedNode))
            source = source.Where(m => m.NodeName == SelectedNode);

        if (!string.IsNullOrEmpty(SelectedTag))
            source = source.Where(m => m.TagList.Contains(SelectedTag));

        if (!string.IsNullOrEmpty(q))
            source = source.Where(m =>
                m.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                m.Vmid.ToString().Contains(q) ||
                m.NodeName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                m.ServerName.Contains(q, StringComparison.OrdinalIgnoreCase));

        var filtered = Sort(source).ToList();

        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            FilteredMachines = new ObservableCollection<MachineData>(filtered);
            GroupedMachines  = new ObservableCollection<NodeGroup>(
                filtered.GroupBy(m => m.NodeName)
                        .OrderBy(g => g.Key)
                        .Select(g => new NodeGroup(g.Key, new ObservableCollection<MachineData>(g))));

            if (!nodes.SequenceEqual(NodeList))
            {
                NodeList.Clear();
                foreach (var n in nodes) NodeList.Add(n);
            }
            IsEmpty = filtered.Count == 0;
        });
    }

    private IEnumerable<MachineData> Sort(IEnumerable<MachineData> src) => SortIndex switch
    {
        1 => src.OrderBy(m => m.Vmid),
        2 => src.OrderByDescending(m => m.Cpu).ThenBy(m => m.Name),
        3 => src.OrderByDescending(m => m.MemPercent).ThenBy(m => m.Name),
        4 => src.OrderBy(m => m.Status).ThenBy(m => m.Name),
        5 => src.OrderByDescending(m => m.Uptime).ThenBy(m => m.Name),
        _ => src.OrderBy(m => m.Name, StringComparer.OrdinalIgnoreCase),
    };

    public event Action<MachineData, string>? OnOpenConsole;
    public event Action<SpiceObject>?         OnOpenSpice;
    public event Action?                      OnLogout;
    public event Action?                      OnRequestAddServer;
    public event Action<string>?              OnNotify;

    public void Dispose()
    {
        _cts.Cancel();
        _refreshTimer.Dispose();
        _ticketTimer.Dispose();
        foreach (var c in Connections) { try { c.Api.Dispose(); } catch { } }
    }
}

public record PowerActionArgs(MachineData Machine, string Action, bool Extra = false);
public record ConsoleArgs(MachineData Machine, string ConsoleType);

/// <summary>An authenticated connection to a single Proxmox cluster.</summary>
public sealed partial class ServerConnection : ObservableObject
{
    public string    Name { get; }
    public ApiClient Api  { get; }
    [ObservableProperty] private int machineCount;

    public ServerConnection(string name, ApiClient api) { Name = name; Api = api; }
}

public sealed class NodeGroup(string nodeName, ObservableCollection<MachineData> machines)
{
    public string NodeName   { get; } = nodeName;
    public ObservableCollection<MachineData> Machines { get; } = machines;
    public int    Count        => Machines.Count;
    public int    RunningCount => Machines.Count(m => m.IsRunning);
    public string Summary      => $"{RunningCount}/{Count} running";
}
