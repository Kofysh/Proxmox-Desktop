using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace ProxmoxDesktop.Converters;

public sealed class NotNullToBoolConverter : IValueConverter
{
    public object Convert(object value, Type t, object p, string l) => value is not null && value is not string s || (value is string str && !string.IsNullOrEmpty(str));
    public object ConvertBack(object value, Type t, object p, string l) => throw new NotImplementedException();
}

public sealed class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type t, object p, string l) => value is true ? Visibility.Visible : Visibility.Collapsed;
    public object ConvertBack(object value, Type t, object p, string l) => value is Visibility.Visible;
}

public sealed class BoolToInverseVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type t, object p, string l) => value is true ? Visibility.Collapsed : Visibility.Visible;
    public object ConvertBack(object value, Type t, object p, string l) => value is Visibility.Collapsed;
}
