using Microsoft.UI.Xaml.Controls;
using ProxmoxDesktop.App.ViewModels;
using ProxmoxDesktop.Core.Config;

namespace ProxmoxDesktop.App.Views;

public sealed partial class LoginPage : Page
{
    public LoginViewModel ViewModel { get; }

    public LoginPage()
    {
        InitializeComponent();
        ViewModel = new LoginViewModel(new ConfigurationService());
        ViewModel.OnLoginSuccess += OnLoginSuccess;
    }

    private void OnLoginSuccess()
    {
        // Passer la page principale avec l'ApiClient validé
        if (App.MainWindow is MainWindow win)
        {
            win.RootFrame.Navigate(typeof(MainPage), ViewModel.ApiClient);
            win.RootFrame.BackStack.Clear();
        }
    }

    private async void Server_LostFocus(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Recharger les realms quand le serveur ou le port change
        await ViewModel.LoadRealmsCommand.ExecuteAsync(null);
    }
}
