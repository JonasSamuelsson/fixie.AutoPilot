using System;
using System.Globalization;

namespace Fixie.AutoRun.Converters
{
   public class HasValueConverter : ConverterBase
   {
      public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         return value != null;
      }

      public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         return null;
      }
   }
}