using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;

namespace ProxmoxDesktop.Services;

public enum ActivityLevel { Info, Success, Warning, Error }

/// <summary>A single, immutable line in the activity log (UI-facing).</summary>
public sealed record ActivityEntry(DateTime Time, PackIconKind Icon, Brush Color, string Title, string? Detail)
{
    public string TimeText => Time.ToString("HH:mm:ss");
}

/// <summary>
/// In-memory, bounded activity log. New entries are inserted at the top and the
/// collection is always mutated on the UI thread so it can be bound directly.
/// </summary>
public sealed class ActivityLogService
{
    private const int MaxEntries = 200;

    public ObservableCollection<ActivityEntry> Entries { get; } = [];

    public void Info(string title, string? detail = null)    => Add(ActivityLevel.Info,    title, detail);
    public void Success(string title, string? detail = null) => Add(ActivityLevel.Success, title, detail);
    public void Warning(string title, string? detail = null) => Add(ActivityLevel.Warning, title, detail);
    public void Error(string title, string? detail = null)   => Add(ActivityLevel.Error,   title, detail);

    public void Add(ActivityLevel level, string title, string? detail = null)
    {
        var (icon, color) = level switch
        {
            ActivityLevel.Success => (PackIconKind.CheckCircle,       Rgb(76, 175, 80)),
            ActivityLevel.Warning => (PackIconKind.AlertCircle,       Rgb(255, 152, 0)),
            ActivityLevel.Error   => (PackIconKind.CloseCircle,       Rgb(244, 67, 54)),
            _                     => (PackIconKind.InformationOutline, Rgb(33, 150, 243)),
        };

        var entry = new ActivityEntry(DateTime.Now, icon, color, title, detail);
        OnUi(() =>
        {
            Entries.Insert(0, entry);
            while (Entries.Count > MaxEntries) Entries.RemoveAt(Entries.Count - 1);
        });
    }

    public void Clear() => OnUi(Entries.Clear);

    private static void OnUi(Action action)
    {
        var app = Application.Current;
        if (app is not null && !app.Dispatcher.CheckAccess()) app.Dispatcher.Invoke(action);
        else action();
    }

    private static SolidColorBrush Rgb(byte r, byte g, byte b)
    {
        var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
        brush.Freeze();
        return brush;
    }
}
