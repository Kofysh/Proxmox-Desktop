using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ProxmoxDesktop.Converters;

public sealed class NotNullToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type t, object? p, CultureInfo l)
        => value is string s ? (string.IsNullOrEmpty(s) ? Visibility.Collapsed : Visibility.Visible)
                             : (value is not null ? Visibility.Visible : Visibility.Collapsed);
    public object ConvertBack(object? v, Type t, object? p, CultureInfo l) => throw new NotImplementedException();
}

public sealed class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type t, object? p, CultureInfo l)
        => value is true ? Visibility.Collapsed : Visibility.Visible;
    public object ConvertBack(object? v, Type t, object? p, CultureInfo l) => throw new NotImplementedException();
}

public sealed class StatusToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type t, object? p, CultureInfo l)
        => value is "running"
            ? new SolidColorBrush(Color.FromRgb(76, 175, 80))
            : new SolidColorBrush(Color.FromRgb(244, 67, 54));
    public object ConvertBack(object? v, Type t, object? p, CultureInfo l) => throw new NotImplementedException();
}

public sealed class BytesToReadableConverter : IValueConverter
{
    public object Convert(object? value, Type t, object? p, CultureInfo l)
    {
        if (value is not long bytes) return "0 MB";
        return bytes >= 1_073_741_824 ? $"{bytes / 1_073_741_824.0:F1} GB" : $"{bytes / 1_048_576.0:F0} MB";
    }
    public object ConvertBack(object? v, Type t, object? p, CultureInfo l) => throw new NotImplementedException();
}

public sealed class CpuPercentConverter : IValueConverter
{
    public object Convert(object? value, Type t, object? p, CultureInfo l)
        => value is double d ? $"{d * 100:F1}%" : "0%";
    public object ConvertBack(object? v, Type t, object? p, CultureInfo l) => throw new NotImplementedException();
}

/// <summary>Inverse bool to Visibility — supports ConverterParameter=inverse for normal direction</summary>
public sealed class BoolToInverseVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type t, object? p, CultureInfo l)
        => value is true ? Visibility.Collapsed : Visibility.Visible;
    public object ConvertBack(object? v, Type t, object? p, CultureInfo l) => throw new NotImplementedException();
}

/// <summary>Visible when the bound collection has at least one item, otherwise Collapsed.</summary>
public sealed class CollectionToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type t, object? p, CultureInfo l)
    {
        if (value is System.Collections.IEnumerable e)
        {
            foreach (var _ in e) return Visibility.Visible;
            return Visibility.Collapsed;
        }
        return value is null ? Visibility.Collapsed : Visibility.Visible;
    }
    public object ConvertBack(object? v, Type t, object? p, CultureInfo l) => throw new NotImplementedException();
}

/// <summary>
/// Maps a bool to a <see cref="GridLength"/> — true → ConverterParameter px (default 320),
/// false → 0. Used to collapse the activity panel column.
/// </summary>
public sealed class BoolToGridLengthConverter : IValueConverter
{
    public object Convert(object? value, Type t, object? p, CultureInfo l)
    {
        var width = 320.0;
        if (p is string s && double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var w))
            width = w;
        return value is true ? new GridLength(width) : new GridLength(0);
    }
    public object ConvertBack(object? v, Type t, object? p, CultureInfo l) => throw new NotImplementedException();
}
