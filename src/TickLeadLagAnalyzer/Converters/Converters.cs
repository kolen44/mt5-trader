using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace TickLeadLagAnalyzer.Converters;

/// <summary>
/// Inverts boolean value.
/// </summary>
public class InverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b ? !b : value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b ? !b : value;
    }
}

/// <summary>
/// Converts boolean to Visibility.
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b && b ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is Visibility v && v == Visibility.Visible;
    }
}

/// <summary>
/// Converts connection status to color.
/// </summary>
public class StatusColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool connected && connected 
            ? new SolidColorBrush(Color.FromRgb(78, 201, 176)) // #4EC9B0 - Green
            : new SolidColorBrush(Color.FromRgb(241, 76, 76)); // #F14C4C - Red
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts IsLeading to icon.
/// </summary>
public class LeadLagIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool isLeading && isLeading ? "✓" : "✗";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts IsLeading to color.
/// </summary>
public class LeadLagColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool isLeading && isLeading
            ? new SolidColorBrush(Color.FromRgb(78, 201, 176)) // #4EC9B0 - Green
            : new SolidColorBrush(Color.FromRgb(241, 76, 76)); // #F14C4C - Red
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
