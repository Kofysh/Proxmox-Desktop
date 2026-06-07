using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ProxmoxDesktop.App.Views;

public sealed partial class MainWindow : Window
{
    public static MainWindow? Instance { get; private set; }

    public MainWindow()
    {
        InitializeComponent();
        Instance = this;
        RootFrame.Navigate(typeof(LoginPage));
    }

    /// <summary>Navigates to the main page after successful authentication.</summary>
    public void NavigateToMain()
    {
        RootFrame.Navigate(typeof(MainPage));
    }

    /// <summary>Returns to the login page (logout).</summary>
    public void NavigateToLogin()
    {
        RootFrame.Navigate(typeof(LoginPage));
        RootFrame.BackStack.Clear();
    }

    /// <summary>Toggles between dark and light theme on the root frame.</summary>
    public void ToggleTheme()
    {
        if (RootFrame.RequestedTheme == ElementTheme.Dark)
            RootFrame.RequestedTheme = ElementTheme.Light;
        else if (RootFrame.RequestedTheme == ElementTheme.Light)
            RootFrame.RequestedTheme = ElementTheme.Dark;
        else
        {
            // Default: follow system — switch to explicit dark first
            RootFrame.RequestedTheme = ElementTheme.Dark;
        }
    }

    /// <summary>Returns the current effective theme (Dark or Light).</summary>
    public ElementTheme CurrentTheme => RootFrame.RequestedTheme == ElementTheme.Default
        ? (Application.Current.RequestedTheme == ApplicationTheme.Dark
            ? ElementTheme.Dark
            : ElementTheme.Light)
        : RootFrame.RequestedTheme;
}
