using System;
using System.Globalization;
using System.Windows.Data;

namespace Fixie.AutoRun.Converters
{
    public class InvertedBoolConverter : IValueConverter
    {
        public static readonly IValueConverter Instance = new InvertedBoolConverter();

        private InvertedBoolConverter() { }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}