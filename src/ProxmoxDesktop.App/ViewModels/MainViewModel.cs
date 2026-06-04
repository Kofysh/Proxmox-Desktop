using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProxmoxDesktop.Core.Api;
using ProxmoxDesktop.Core.Api.Models;

namespace ProxmoxDesktop.App.ViewModels;

public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly ApiClient _api;
    private readonly PeriodicTimer _refreshTimer;
    private readonly PeriodicTimer _ticketTimer;
    private CancellationTokenSource _cts = new();

    // -------------------------------------------------------------------------
    // State
    // -------------------------------------------------------------------------

    [ObservableProperty]
    private ObservableCollection<MachineData> _machines = [];

    [ObservableProperty]
    private ObservableCollection<MachineData> _filteredMachines = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private string? _statusMessage;

    partial void OnSearchQueryChanged(string value) => ApplyFilter();

    // -------------------------------------------------------------------------
    // Init
    // -------------------------------------------------------------------------

    public MainViewModel(ApiClient api)
    {
        _api = api;

        // Timer de refresh UI : toutes les 60 secondes
        _refreshTimer = new PeriodicTimer(TimeSpan.FromSeconds(60));
        // Timer de renouvellement de ticket : toutes les 90 minutes
        _ticketTimer = new PeriodicTimer(TimeSpan.FromMinutes(90));

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
            // Mise à jour différentielle : évite de recréer toute la liste
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
            // Supprimer les VMs qui n'existent plus
            var toRemove = Machines.Where(m => all.All(a => a.Vmid != m.Vmid)).ToList();
            foreach (var m in toRemove) Machines.Remove(m);
            ApplyFilter();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { StatusMessage = $"Erreur de chargement : {ex.Message}"; }
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
            PowerResult.Forbidden => "Permission refusée (VM.PowerMgmt requis).",
            PowerResult.Error     => "Erreur lors de l'action d'alimentation.",
            _                    => null
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
        string? proxy = null; // TODO: lire depuis ConfigurationService si configuré
        var spiceConfig = await _api.GetSpiceConfigAsync(machine, proxy);
        if (spiceConfig is not null)
            OnOpenSpice?.Invoke(spiceConfig);
    }

    // -------------------------------------------------------------------------
    // Filtrage
    // -------------------------------------------------------------------------

    private void ApplyFilter()
    {
        var q = SearchQuery.Trim().ToLowerInvariant();
        var filtered = string.IsNullOrEmpty(q)
            ? Machines
            : new ObservableCollection<MachineData>(
                Machines.Where(m =>
                    m.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    m.Vmid.ToString().Contains(q) ||
                    m.NodeName.Contains(q, StringComparison.OrdinalIgnoreCase)));

        FilteredMachines = filtered is ObservableCollection<MachineData> oc
            ? oc
            : new ObservableCollection<MachineData>(filtered);
    }

    // -------------------------------------------------------------------------
    // Events vers la View
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
