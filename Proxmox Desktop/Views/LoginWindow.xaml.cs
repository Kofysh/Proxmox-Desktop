using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ProxmoxDesktop.Api;
using ProxmoxDesktop.Config;
using ProxmoxDesktop.ViewModels;

namespace ProxmoxDesktop.Views;

public sealed partial class LoginWindow : Window
{
    public LoginViewModel ViewModel { get; }
    private readonly Action<IApiClient>? _onConnected;

    public LoginWindow(Action<IApiClient>? onConnected = null)
    {
        InitializeComponent();
        _onConnected = onConnected;
        ViewModel = new LoginViewModel(new ConfigurationService());
        ContentRoot.DataContext = ViewModel;
        ViewModel.OnLoginSuccess += OnLoginSuccess;
        AppWindow.Resize(new Windows.Graphics.SizeInt32(560, 760));
    }

    private void OnLoginSuccess(IApiClient api)
    {
        if (_onConnected is not null) _onConnected(api);
        else App.ShowMainWindow(api);
        Close();
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        => ViewModel.Password = ((PasswordBox)sender).Password;

    private void TokenSecretBox_PasswordChanged(object sender, RoutedEventArgs e)
        => ViewModel.TokenSecret = ((PasswordBox)sender).Password;
}
