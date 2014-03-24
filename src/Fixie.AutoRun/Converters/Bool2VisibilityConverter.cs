using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Fixie.AutoRun.Converters
{
   public class Bool2VisibilityConverter : IValueConverter
   {
      private readonly Visibility _falseValue;
      private readonly Visibility _trueValue;
      public static readonly IValueConverter Instance = new Bool2VisibilityConverter(Visibility.Collapsed, Visibility.Visible);
      public static readonly IValueConverter InvertedInstance = new Bool2VisibilityConverter(Visibility.Visible, Visibility.Collapsed);

      private Bool2VisibilityConverter(Visibility falseValue, Visibility trueValue)
      {
         _falseValue = falseValue;
         _trueValue = trueValue;
      }

      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         return (bool)value ? _trueValue : _falseValue;
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         throw new NotImplementedException();
      }
   }
}