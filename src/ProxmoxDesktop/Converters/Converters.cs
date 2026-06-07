using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ProxmoxDesktop.Converters;

/// <summary>Returns Visible when value is not null/empty string.</summary>
public sealed class NotNullToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type t, object? p, CultureInfo l)
        => value is string s ? (string.IsNullOrEmpty(s) ? Visibility.Collapsed : Visibility.Visible)
                             : (value is not null ? Visibility.Visible : Visibility.Collapsed);
    public object ConvertBack(object? v, Type t, object? p, CultureInfo l) => throw new NotImplementedException();
}

/// <summary>Returns Collapsed when bool is true (inverse of BooleanToVisibilityConverter).</summary>
public sealed class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type t, object? p, CultureInfo l)
        => value is true ? Visibility.Collapsed : Visibility.Visible;
    public object ConvertBack(object? v, Type t, object? p, CultureInfo l) => throw new NotImplementedException();
}

/// <summary>Returns green brush when running, red otherwise.</summary>
public sealed class StatusToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type t, object? p, CultureInfo l)
        => value is "running"
            ? new SolidColorBrush(Color.FromRgb(76, 175, 80))
            : new SolidColorBrush(Color.FromRgb(244, 67, 54));
    public object ConvertBack(object? v, Type t, object? p, CultureInfo l) => throw new NotImplementedException();
}

/// <summary>Converts bytes to human-readable string (MB / GB).</summary>
public sealed class BytesToReadableConverter : IValueConverter
{
    public object Convert(object? value, Type t, object? p, CultureInfo l)
    {
        if (value is not long bytes) return "0 MB";
        return bytes >= 1_073_741_824
            ? $"{bytes / 1_073_741_824.0:F1} GB"
            : $"{bytes / 1_048_576.0:F0} MB";
    }
    public object ConvertBack(object? v, Type t, object? p, CultureInfo l) => throw new NotImplementedException();
}

/// <summary>Formats CPU usage: 0.05 → "5.0%"</summary>
public sealed class CpuPercentConverter : IValueConverter
{
    public object Convert(object? value, Type t, object? p, CultureInfo l)
        => value is double d ? $"{d * 100:F1}%" : "0%";
    public object ConvertBack(object? v, Type t, object? p, CultureInfo l) => throw new NotImplementedException();
}
