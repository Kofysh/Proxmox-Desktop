using System.Windows;
using System.Windows.Controls;
using ProxmoxDesktop.Config;
using ProxmoxDesktop.ViewModels;

namespace ProxmoxDesktop.Views;

public partial class LoginWindow : Window
{
    public LoginViewModel ViewModel { get; }

    public LoginWindow()
    {
        InitializeComponent();
        ViewModel   = new LoginViewModel(new ConfigurationService());
        DataContext = ViewModel;
        ViewModel.OnLoginSuccess += api => { new MainWindow(api).Show(); Close(); };
    }

    private void Server_LostFocus(object sender, RoutedEventArgs e)         => ViewModel.LoadRealmsCommand.Execute(null);
    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)    => ViewModel.Password    = ((PasswordBox)sender).Password;
    private void TokenSecretBox_PasswordChanged(object sender, RoutedEventArgs e) => ViewModel.TokenSecret = ((PasswordBox)sender).Password;
}
