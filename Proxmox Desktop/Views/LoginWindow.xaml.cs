using System.Windows;
using ProxmoxDesktop.Api;
using ProxmoxDesktop.Config;
using ProxmoxDesktop.ViewModels;

namespace ProxmoxDesktop.Views;

public partial class LoginWindow : Window
{
    public LoginViewModel ViewModel { get; }
    private readonly Action<IApiClient>? _onConnected;

    public LoginWindow(Action<IApiClient>? onConnected = null)
    {
        InitializeComponent();
        _onConnected = onConnected;
        ViewModel = new LoginViewModel(new ConfigurationService());
        DataContext = ViewModel;
        ViewModel.OnLoginSuccess += OnLoginSuccess;
    }

    private void OnLoginSuccess(IApiClient api)
    {
        if (_onConnected is not null) _onConnected(api);
        else App.ShowMainWindow(api);
        Close();
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        => ViewModel.Password = PasswordBox.Password;

    private void TokenSecretBox_PasswordChanged(object sender, RoutedEventArgs e)
        => ViewModel.TokenSecret = TokenSecretBox.Password;

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(ViewModel.Server))
            await ViewModel.LoadRealmsAsync();
    }

    private async void Realm_DropDownOpened(object sender, EventArgs e)
    {
        if (ViewModel.Realms.Count == 0 && !ViewModel.IsBusy)
            await ViewModel.LoadRealmsAsync();
    }
}
