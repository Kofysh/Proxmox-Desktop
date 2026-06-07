using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ProxmoxDesktop.Api;

namespace ProxmoxDesktop.Views;

public sealed partial class MainWindow : Window
{
    public static MainWindow? Instance { get; private set; }

    public MainWindow()
    {
        InitializeComponent();
        Instance = this;
        RootFrame.Navigate(typeof(LoginPage));
    }

    /// <summary>Navigate to main page, passing the authenticated ApiClient as parameter.</summary>
    public void NavigateToMain(ApiClient api) => RootFrame.Navigate(typeof(MainPage), api);

    /// <summary>Return to login page.</summary>
    public void NavigateToLogin()
    {
        RootFrame.Navigate(typeof(LoginPage));
        RootFrame.BackStack.Clear();
    }

    /// <summary>Toggle dark/light theme on the root frame.</summary>
    public void ToggleTheme()
    {
        RootFrame.RequestedTheme = RootFrame.RequestedTheme switch
        {
            ElementTheme.Dark    => ElementTheme.Light,
            ElementTheme.Light   => ElementTheme.Dark,
            _                    => ElementTheme.Dark   // Default → Dark first
        };
    }

    public ElementTheme CurrentTheme => RootFrame.RequestedTheme == ElementTheme.Default
        ? (Application.Current.RequestedTheme == ApplicationTheme.Dark ? ElementTheme.Dark : ElementTheme.Light)
        : RootFrame.RequestedTheme;
}
