using System.Collections.ObjectModel;
using Microsoft.UI.Dispatching;

namespace ProxmoxDesktop.Services;

public enum ActivityLevel { Info, Success, Warning, Error }

public sealed record ActivityEntry(DateTime Time, string Icon, string Title, string? Detail)
{
    public string TimeText => Time.ToString("HH:mm:ss");
}

public sealed class ActivityLogService
{
    private const int MaxEntries = 200;
    private readonly DispatcherQueue? _dispatcher;
    public ObservableCollection<ActivityEntry> Entries { get; } = [];

    public ActivityLogService(DispatcherQueue? dispatcher = null) => _dispatcher = dispatcher;
    public void Info(string title, string? detail = null) => Add(ActivityLevel.Info, title, detail);
    public void Success(string title, string? detail = null) => Add(ActivityLevel.Success, title, detail);
    public void Warning(string title, string? detail = null) => Add(ActivityLevel.Warning, title, detail);
    public void Error(string title, string? detail = null) => Add(ActivityLevel.Error, title, detail);

    public void Add(ActivityLevel level, string title, string? detail = null)
    {
        var icon = level switch { ActivityLevel.Success => "✓", ActivityLevel.Warning => "!", ActivityLevel.Error => "×", _ => "i" };
        void Update()
        {
            Entries.Insert(0, new ActivityEntry(DateTime.Now, icon, title, detail));
            while (Entries.Count > MaxEntries) Entries.RemoveAt(Entries.Count - 1);
        }
        if (_dispatcher is null || _dispatcher.HasThreadAccess) Update();
        else _dispatcher.TryEnqueue(Update);
    }

    public void Clear()
    {
        if (_dispatcher is null || _dispatcher.HasThreadAccess) Entries.Clear();
        else _dispatcher.TryEnqueue(Entries.Clear);
    }
}
