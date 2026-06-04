using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using CommunityToolkit.Mvvm.Input;
using ProxmoxDesktop.App.ViewModels;
using ProxmoxDesktop.Core.Api.Models;

namespace ProxmoxDesktop.App.Controls;

public sealed partial class MachineCard : UserControl
{
    // -------------------------------------------------------------------------
    // DependencyProperty
    // -------------------------------------------------------------------------

    public static readonly DependencyProperty MachineProperty =
        DependencyProperty.Register(nameof(Machine), typeof(MachineData),
            typeof(MachineCard), new PropertyMetadata(null, OnMachineChanged));

    public MachineData Machine
    {
        get => (MachineData)GetValue(MachineProperty);
        set => SetValue(MachineProperty, value);
    }

    private static void OnMachineChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((MachineCard)d).Bindings.Update();

    // -------------------------------------------------------------------------
    // Computed pour {x:Bind}
    // -------------------------------------------------------------------------

    public SolidColorBrush StatusBrush => Machine?.IsRunning == true
        ? new SolidColorBrush(Colors.ForestGreen)
        : new SolidColorBrush(Colors.Crimson);

    public string MachineIcon => Machine?.IsLxc == true
        ? "/Assets/lxc.png"
        : "/Assets/vm.png";

    public bool CanStart    => Machine is { IsRunning: false } m && m.Lock != "suspended";
    public bool CanResume   => Machine is { IsRunning: false } m && m.Lock == "suspended" && !m.IsLxc;
    public bool CanSuspend  => Machine?.IsRunning == true && Machine?.IsLxc == false;
    public bool CanReset    => Machine?.IsRunning == true && Machine?.IsLxc == false;
    public bool CanOpenXterm => Machine is { } m && (m.IsLxc || m.Serial == 1);

    // -------------------------------------------------------------------------
    // Commands — délégués au MainViewModel via le DataContext
    // -------------------------------------------------------------------------

    private MainViewModel? VM => (DataContext as MainViewModel)
        ?? (this.FindParent<Page>()?.DataContext as MainViewModel);

    public IRelayCommand OpenNoVncCommand   => new RelayCommand(() => VM?.OpenConsoleCommand.Execute(new ConsoleArgs(Machine, "novnc")));
    public IRelayCommand OpenXtermCommand   => new RelayCommand(() => VM?.OpenConsoleCommand.Execute(new ConsoleArgs(Machine, "xtermjs")));
    public IRelayCommand OpenSpiceCommand   => new RelayCommand(() => VM?.OpenSpiceCommand.Execute(Machine));
    public IRelayCommand StartCommand       => new RelayCommand(() => VM?.PowerActionCommand.Execute(new PowerActionArgs(Machine, "start")));
    public IRelayCommand ShutdownCommand    => new RelayCommand(() => VM?.PowerActionCommand.Execute(new PowerActionArgs(Machine, "shutdown")));
    public IRelayCommand RebootCommand      => new RelayCommand(() => VM?.PowerActionCommand.Execute(new PowerActionArgs(Machine, "reboot")));
    public IRelayCommand SuspendCommand     => new RelayCommand(() => VM?.PowerActionCommand.Execute(new PowerActionArgs(Machine, "suspend")));
    public IRelayCommand ResumeCommand      => new RelayCommand(() => VM?.PowerActionCommand.Execute(new PowerActionArgs(Machine, "start")));
    public IRelayCommand HibernateCommand   => new RelayCommand(() => VM?.PowerActionCommand.Execute(new PowerActionArgs(Machine, "suspend", Extra: true)));
    public IRelayCommand StopCommand        => new RelayCommand(() => VM?.PowerActionCommand.Execute(new PowerActionArgs(Machine, "stop")));
    public IRelayCommand ResetCommand       => new RelayCommand(() => VM?.PowerActionCommand.Execute(new PowerActionArgs(Machine, "reset")));

    public MachineCard() => InitializeComponent();
}
