using System.Windows;
using System.Windows.Media;

namespace ProxmoxDesktop.Helpers;

public static class VisualTreeHelperExtensions
{
    public static T? FindParent<T>(this DependencyObject child) where T : DependencyObject
    {
        var parent = VisualTreeHelper.GetParent(child);
        while (parent is not null) { if (parent is T found) return found; parent = VisualTreeHelper.GetParent(parent); }
        return null;
    }
}
