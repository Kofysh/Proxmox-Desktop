using System.Windows;
using System.Windows.Controls;

namespace ProxmoxDesktop.Controls;

public partial class StatsBar : UserControl
{
    public static readonly DependencyProperty TotalCountProperty   = DependencyProperty.Register(nameof(TotalCount),   typeof(int), typeof(StatsBar));
    public static readonly DependencyProperty RunningCountProperty = DependencyProperty.Register(nameof(RunningCount), typeof(int), typeof(StatsBar));
    public static readonly DependencyProperty StoppedCountProperty = DependencyProperty.Register(nameof(StoppedCount), typeof(int), typeof(StatsBar));
    public static readonly DependencyProperty VmCountProperty      = DependencyProperty.Register(nameof(VmCount),      typeof(int), typeof(StatsBar));
    public static readonly DependencyProperty LxcCountProperty     = DependencyProperty.Register(nameof(LxcCount),     typeof(int), typeof(StatsBar));

    public int TotalCount   { get => (int)GetValue(TotalCountProperty);   set => SetValue(TotalCountProperty,   value); }
    public int RunningCount { get => (int)GetValue(RunningCountProperty); set => SetValue(RunningCountProperty, value); }
    public int StoppedCount { get => (int)GetValue(StoppedCountProperty); set => SetValue(StoppedCountProperty, value); }
    public int VmCount      { get => (int)GetValue(VmCountProperty);      set => SetValue(VmCountProperty,      value); }
    public int LxcCount     { get => (int)GetValue(LxcCountProperty);     set => SetValue(LxcCountProperty,     value); }

    public StatsBar() => InitializeComponent();
}
