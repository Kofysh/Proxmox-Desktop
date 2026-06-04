using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace ProxmoxDesktop.App.Converters;

/// <summary>Convertit un objet nullable en bool (true si non null) et en Visibility.</summary>
public class NotNullToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool isNotNull = value is string s ? !string.IsNullOrEmpty(s) : value is not null;
        if (targetType == typeof(Visibility))
            return isNotNull ? Visibility.Visible : Visibility.Collapsed;
        return isNotNull;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
