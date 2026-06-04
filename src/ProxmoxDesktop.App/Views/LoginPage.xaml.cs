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
        // Utilise NavigateToMain() exposé publiquement par MainWindow
        if (App.MainWindow is MainWindow win)
        {
            win.NavigateToMain();
        }
    }

    private async void Server_LostFocus(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        await ViewModel.LoadRealmsCommand.ExecuteAsync(null);
    }
}
