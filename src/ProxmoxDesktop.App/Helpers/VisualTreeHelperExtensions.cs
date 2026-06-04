using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace ProxmoxDesktop.App.Helpers;

public static class VisualTreeHelperExtensions
{
    /// <summary>Remonte l'arbre visuel pour trouver un parent du type T.</summary>
    public static T? FindParent<T>(this DependencyObject child) where T : DependencyObject
    {
        var parent = VisualTreeHelper.GetParent(child);
        while (parent is not null)
        {
            if (parent is T target) return target;
            parent = VisualTreeHelper.GetParent(parent);
        }
        return null;
    }
}
