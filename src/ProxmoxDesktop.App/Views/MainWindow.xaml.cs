using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ProxmoxDesktop.App.Views;

public sealed partial class MainWindow : Window
{
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
