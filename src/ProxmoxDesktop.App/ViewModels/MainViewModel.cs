using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProxmoxDesktop.App.Services;
using ProxmoxDesktop.Core.Api;
using ProxmoxDesktop.Core.Api.Models;

namespace ProxmoxDesktop.App.ViewModels;

public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly ApiClient _api;
    private readonly PeriodicTimer _refreshTimer;
    private readonly PeriodicTimer _ticketTimer;
    private CancellationTokenSource _cts = new();

    // Track previous statuses to detect changes
    private Dictionary<int, string> _previousStatuses = [];

    // -------------------------------------------------------------------------
    // State
    // -------------------------------------------------------------------------

    [ObservableProperty]
    public partial ObservableCollection<MachineData> Machines { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<NodeGroup> GroupedMachines { get; set; } = [];

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial string SearchQuery { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? StatusMessage { get; set; }

    [ObservableProperty]
    public partial bool IsGroupedByNode { get; set; } = false;

    partial void OnSearchQueryChanged(string value) => ApplyFilter();
    partial void OnIsGroupedByNodeChanged(bool value) => ApplyFilter();

    // -------------------------------------------------------------------------
    // Init
    // -------------------------------------------------------------------------

    public MainViewModel(ApiClient api)
    {
        _api = api;
        _refreshTimer = new PeriodicTimer(TimeSpan.FromSeconds(60));
        _ticketTimer  = new PeriodicTimer(TimeSpan.FromMinutes(90));
        StartTimers();
    }

    private void StartTimers()
    {
        _ = Task.Run(async () =>
        {
            while (await _refreshTimer.WaitForNextTickAsync(_cts.Token))
                await RefreshAsync(_cts.Token);
        });

        _ = Task.Run(async () =>
        {
            while (await _ticketTimer.WaitForNextTickAsync(_cts.Token))
                await _api.RenewTicketAsync(_cts.Token);
        });
    }

    // -------------------------------------------------------------------------
    // Commands
    // -------------------------------------------------------------------------

    [RelayCommand]
    public async Task RefreshAsync(CancellationToken ct = default)
    {
        IsLoading = true;
        StatusMessage = null;
        try
        {
            var all = await _api.GetAllMachinesAsync(ct);

            // Detect state changes before updating the list
            // Skip notifications on first load (_previousStatuses is empty)
            bool isFirstLoad = _previousStatuses.Count == 0;

            foreach (var m in all)
            {
                if (!isFirstLoad &&
                    _previousStatuses.TryGetValue(m.Vmid, out var prevStatus) &&
                    prevStatus != m.Status)
                {
                    NotificationService.NotifyStateChange(m.Name, m.Vmid, prevStatus, m.Status);
                }
            }

            // Update status snapshot
            _previousStatuses = all.ToDictionary(m => m.Vmid, m => m.Status);

            // Differential update of the observable collection
            var existing = Machines.ToDictionary(m => m.Vmid);
            foreach (var m in all)
            {
                if (existing.TryGetValue(m.Vmid, out _))
                {
                    var idx = Machines.IndexOf(Machines.First(x => x.Vmid == m.Vmid));
                    Machines[idx] = m;
                }
                else Machines.Add(m);
            }
            var toRemove = Machines.Where(m => all.All(a => a.Vmid != m.Vmid)).ToList();
            foreach (var m in toRemove) Machines.Remove(m);

            ApplyFilter();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { StatusMessage = $"Load error: {ex.Message}"; }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    public async Task PowerActionAsync(PowerActionArgs args)
    {
        var result = await _api.PowerActionAsync(
            args.Machine, args.Action,
            args.Action == "suspend" && args.Extra);

        StatusMessage = result switch
        {
            PowerResult.Forbidden => "Permission denied (VM.PowerMgmt required).",
            PowerResult.Error     => "Power action failed.",
            _                     => null
        };

        await Task.Delay(3000);
        await RefreshAsync();
    }

    [RelayCommand]
    public async Task OpenConsoleAsync(ConsoleArgs args)
    {
        var url = await _api.GetConsoleUrlAsync(args.Machine, args.ConsoleType);
        if (url is not null)
            OnOpenConsole?.Invoke(args.Machine, url);
    }

    [RelayCommand]
    public async Task OpenSpiceAsync(MachineData machine)
    {
        var spiceConfig = await _api.GetSpiceConfigAsync(machine);
        if (spiceConfig is not null)
            OnOpenSpice?.Invoke(spiceConfig);
    }

    [RelayCommand]
    public void ToggleGroupByNode() => IsGroupedByNode = !IsGroupedByNode;

    // -------------------------------------------------------------------------
    // Filtering & grouping
    // -------------------------------------------------------------------------

    private void ApplyFilter()
    {
        var q = SearchQuery.Trim().ToLowerInvariant();
        var filtered = string.IsNullOrEmpty(q)
            ? Machines.ToList()
            : Machines.Where(m =>
                m.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                m.Vmid.ToString().Contains(q) ||
                m.NodeName.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();

        var groups = filtered
            .GroupBy(m => m.NodeName)
            .OrderBy(g => g.Key)
            .Select(g => new NodeGroup(g.Key, new ObservableCollection<MachineData>(g.OrderBy(m => m.Vmid))))
            .ToList();

        GroupedMachines = new ObservableCollection<NodeGroup>(groups);
    }

    // -------------------------------------------------------------------------
    // Events
    // -------------------------------------------------------------------------

    public event Action<MachineData, string>? OnOpenConsole;
    public event Action<SpiceObject>? OnOpenSpice;
    public event Action? OnLogout;

    [RelayCommand]
    public void Logout() => OnLogout?.Invoke();

    // -------------------------------------------------------------------------
    // IDisposable
    // -------------------------------------------------------------------------

    public void Dispose()
    {
        _cts.Cancel();
        _refreshTimer.Dispose();
        _ticketTimer.Dispose();
        _api.Dispose();
    }
}

public record PowerActionArgs(MachineData Machine, string Action, bool Extra = false);
public record ConsoleArgs(MachineData Machine, string ConsoleType);

public sealed class NodeGroup(string nodeName, ObservableCollection<MachineData> machines)
{
    public string NodeName { get; } = nodeName;
    public ObservableCollection<MachineData> Machines { get; } = machines;
    public int Count => Machines.Count;
    public int RunningCount => Machines.Count(m => m.IsRunning);
    public string Summary => $"{RunningCount}/{Count} running";
}
