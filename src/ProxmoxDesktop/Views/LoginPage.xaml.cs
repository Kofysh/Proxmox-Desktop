using Microsoft.UI.Xaml.Controls;
using ProxmoxDesktop.Config;
using ProxmoxDesktop.ViewModels;

namespace ProxmoxDesktop.Views;

public sealed partial class LoginPage : Page
{
    public LoginViewModel ViewModel { get; }

    public LoginPage()
    {
        InitializeComponent();
        ViewModel = new LoginViewModel(new ConfigurationService());
        // Pass the authenticated ApiClient to MainWindow.NavigateToMain
        ViewModel.OnLoginSuccess += api =>
        {
            if (App.MainWindow is MainWindow win)
                win.NavigateToMain(api);
        };
    }

    private async void Server_LostFocus(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        => await ViewModel.LoadRealmsCommand.ExecuteAsync(null);
}
