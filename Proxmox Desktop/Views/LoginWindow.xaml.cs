using System.Windows;
using System.Windows.Controls;
using ProxmoxDesktop.Api;
using ProxmoxDesktop.Config;
using ProxmoxDesktop.ViewModels;

namespace ProxmoxDesktop.Views;

public partial class LoginWindow : Window
{
    public LoginViewModel ViewModel { get; }

    /// <summary>
    /// Default constructor: on success, opens the main window.
    /// When <paramref name="onConnected"/> is supplied, the window acts as an
    /// "add connection" dialog — on success it hands the authenticated client
    /// back to the caller and closes, without opening a new main window.
    /// </summary>
    public LoginWindow(Action<IApiClient>? onConnected = null)
    {
        InitializeComponent();
        ViewModel   = new LoginViewModel(new ConfigurationService());
        DataContext = ViewModel;

        if (onConnected is null)
        {
            ViewModel.OnLoginSuccess += api => { new MainWindow(api).Show(); Close(); };
        }
        else
        {
            Title = "Add Proxmox connection";
            ViewModel.OnLoginSuccess += api => { onConnected(api); Close(); };
        }
    }

    private void Server_LostFocus(object sender, RoutedEventArgs e)         => ViewModel.LoadRealmsCommand.Execute(null);
    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)    => ViewModel.Password    = ((PasswordBox)sender).Password;
    private void TokenSecretBox_PasswordChanged(object sender, RoutedEventArgs e) => ViewModel.TokenSecret = ((PasswordBox)sender).Password;
}
