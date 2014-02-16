using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Fixie.AutoPilot
{
    public class Bool2VisibilityConverter : IValueConverter
    {
        public static readonly IValueConverter Instance = new BooleanToVisibilityConverter();

        private Bool2VisibilityConverter() { }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}