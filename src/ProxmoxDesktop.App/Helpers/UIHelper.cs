using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace ProxmoxDesktop.App.Helpers;

/// <summary>Méthodes d'extension utilitaires pour le visual tree WinUI 3.</summary>
public static class UIHelper
{
    /// <summary>
    /// Remonte le visual tree pour trouver le premier parent du type <typeparamref name="T"/>.
    /// Retourne null si aucun parent correspondant n'est trouvé.
    /// </summary>
    public static T? FindParent<T>(this DependencyObject element) where T : DependencyObject
    {
        var parent = VisualTreeHelper.GetParent(element);
        while (parent is not null)
        {
            if (parent is T match)
                return match;
            parent = VisualTreeHelper.GetParent(parent);
        }
        return null;
    }
}
