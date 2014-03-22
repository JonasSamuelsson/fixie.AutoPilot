using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Fixie.AutoRun
{
   public class TextWrapping2BoolConvnverter : IValueConverter
   {
      public static readonly IValueConverter Instance = new TextWrapping2BoolConvnverter();

      private TextWrapping2BoolConvnverter() { }

      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         return (TextWrapping)value == TextWrapping.Wrap;
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         return (bool) value ? TextWrapping.Wrap : TextWrapping.NoWrap;
      }
   }
}