using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.Input;
using ProxmoxDesktop.Api.Models;
using ProxmoxDesktop.ViewModels;

namespace ProxmoxDesktop.Controls;

public partial class MachineCard : UserControl
{
    public static readonly DependencyProperty MachineProperty =
        DependencyProperty.Register(nameof(Machine), typeof(MachineData),
            typeof(MachineCard), new PropertyMetadata(null));

    public MachineData Machine
    {
        get => (MachineData)GetValue(MachineProperty);
        set => SetValue(MachineProperty, value);
    }

    private MainViewModel? VM
    {
        get
        {
            var win = System.Windows.Window.GetWindow(this);
            return win?.DataContext as MainViewModel;
        }
    }

    public string MachineIcon => Machine?.IsLxc == true ? "/Assets/lxc.png" : "/Assets/vm.png";

    public bool CanStart    => Machine is { IsRunning: false } m && m.Lock != "suspended";
    public bool CanResume   => Machine is { IsRunning: false } m && m.Lock == "suspended" && !m.IsLxc;
    public bool CanSuspend  => Machine?.IsRunning == true && Machine?.IsLxc == false;
    public bool CanReset    => Machine?.IsRunning == true && Machine?.IsLxc == false;
    public bool CanOpenXterm=> Machine is { } m && (m.IsLxc || m.Serial >= 1);

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
