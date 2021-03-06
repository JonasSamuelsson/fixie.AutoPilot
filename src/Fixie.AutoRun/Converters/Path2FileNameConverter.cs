using System;
using System.Globalization;
using System.IO;

namespace Fixie.AutoRun.Converters
{
   public class Path2FileNameConverter : ConverterBase
   {
      public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         return Path.GetFileNameWithoutExtension((string)value);
      }
   }
}