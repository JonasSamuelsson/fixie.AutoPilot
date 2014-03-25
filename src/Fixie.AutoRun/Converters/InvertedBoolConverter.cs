using System;
using System.Globalization;
using System.Windows.Data;

namespace Fixie.AutoRun.Converters
{
   public class InvertedBoolConverter : ConverterBase
   {
      public IValueConverter Converter { get; set; }

      public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         return Converter == null
                   ? !(bool)value
                   : Converter.Convert(!(bool)value, targetType, parameter, culture);
      }

      public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         return Converter == null
                   ? !(bool)value
                   : !(bool)Converter.ConvertBack(value, targetType, parameter, culture);
      }
   }
}