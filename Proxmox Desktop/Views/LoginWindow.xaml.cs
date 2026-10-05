using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
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
        var contentRoot = BuildInterface();
        contentRoot.DataContext = ViewModel;
        Content = contentRoot;
        ViewModel.OnLoginSuccess += OnLoginSuccess;
        AppWindow.Resize(new Windows.Graphics.SizeInt32(560, 760));
    }

    private Grid BuildInterface()
    {
        var root = new Grid { Padding = new Thickness(40) };
        var panel = new StackPanel { Spacing = 12 };
        root.Children.Add(new ScrollViewer { Content = panel });

        panel.Children.Add(new TextBlock
        {
            Text = "PROXMOX DESKTOP",
            FontSize = 28,
            FontWeight = Microsoft.UI.Text.FontWeights.Bold,
            Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 59, 130, 246))
        });
        panel.Children.Add(new TextBlock
        {
            Text = "Manage your virtual infrastructure",
            FontSize = 16
        });
        panel.Children.Add(new TextBlock
        {
            Text = "Connect to a Proxmox VE cluster to continue",
            TextWrapping = TextWrapping.Wrap
        });

        panel.Children.Add(BindTextBox("Server address", nameof(ViewModel.Server), "pve.example.com"));
        panel.Children.Add(BindTextBox("Port", nameof(ViewModel.Port), "8006"));

        var realm = new ComboBox { Header = "Realm", ItemsSource = ViewModel.Realms };
        realm.DisplayMemberPath = "Realm";
        realm.SetBinding(ComboBox.SelectedItemProperty, Binding(nameof(ViewModel.SelectedRealm)));
        panel.Children.Add(realm);

        panel.Children.Add(BindTextBox("Username", nameof(ViewModel.Username)));

        var password = new PasswordBox { Header = "Password" };
        password.PasswordChanged += PasswordBox_PasswordChanged;
        panel.Children.Add(password);

        panel.Children.Add(BindTextBox("TOTP code", nameof(ViewModel.Otp)));
        panel.Children.Add(BindTextBox("API token ID", nameof(ViewModel.TokenId), "user@realm!tokenid"));

        var secret = new PasswordBox { Header = "API token secret" };
        secret.PasswordChanged += TokenSecretBox_PasswordChanged;
        panel.Children.Add(secret);

        var skipSsl = new CheckBox { Content = "Ignore SSL certificate" };
        skipSsl.SetBinding(CheckBox.IsCheckedProperty, Binding(nameof(ViewModel.SkipSsl)));
        panel.Children.Add(skipSsl);

        var error = new TextBlock { TextWrapping = TextWrapping.Wrap };
        error.SetBinding(TextBlock.TextProperty, Binding(nameof(ViewModel.ErrorMessage)));
        panel.Children.Add(error);

        var connect = new Button { Content = "Connect", Height = 44 };
        connect.SetBinding(Button.CommandProperty, Binding(nameof(ViewModel.LoginCommand)));
        panel.Children.Add(connect);

        var busy = new ProgressRing { Width = 24, Height = 24 };
        busy.SetBinding(ProgressRing.IsActiveProperty, Binding(nameof(ViewModel.IsBusy)));
        panel.Children.Add(busy);
        return root;
    }

    private TextBox BindTextBox(string header, string property, string? placeholder = null)
    {
        var box = new TextBox { Header = header, PlaceholderText = placeholder ?? string.Empty };
        box.SetBinding(TextBox.TextProperty, Binding(property));
        return box;
    }

    private static Binding Binding(string path)
        => new() { Path = new PropertyPath(path), Mode = BindingMode.TwoWay };

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
