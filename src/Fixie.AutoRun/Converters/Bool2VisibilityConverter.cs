using System;
using System.Globalization;
using System.Windows;

namespace Fixie.AutoRun.Converters
{
   public class Bool2VisibilityConverter : ConverterBase
   {
      public Bool2VisibilityConverter()
      {
         False = Visibility.Collapsed;
      }

      public Visibility False { get; set; }

      public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         return (bool)value ? Visibility.Visible : False;
      }
   }
}