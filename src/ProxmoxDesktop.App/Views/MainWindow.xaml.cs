using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ProxmoxDesktop.App.Views;

namespace ProxmoxDesktop.App.Views;

public sealed partial class MainWindow : Window
{
    /// <summary>Frame racine de navigation, accessible par les pages enfants.</summary>
    public Frame RootFrame => (Frame)Content;

    public MainWindow()
    {
        InitializeComponent();
        // Démarrer sur la page de login
        RootFrame.Navigate(typeof(LoginPage));
    }

    /// <summary>Navigue vers la page principale après authentification réussie.</summary>
    public void NavigateToMain()
    {
        RootFrame.Navigate(typeof(MainPage));
    }

    /// <summary>Revient à la page de login (déconnexion).</summary>
    public void NavigateToLogin()
    {
        RootFrame.Navigate(typeof(LoginPage));
        RootFrame.BackStack.Clear();
    }
}
