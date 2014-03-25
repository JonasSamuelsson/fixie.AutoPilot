using System;
using System.Globalization;
using System.Windows;

namespace Fixie.AutoRun.Converters
{
   public class Bool2TextWrappingConverter : ConverterBase
   {
      public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         return (bool)value ? TextWrapping.Wrap : TextWrapping.NoWrap;
      }
   }
}