using System;
using System.Globalization;
using Fixie.AutoRun.Workers;

namespace Fixie.AutoRun.Converters
{
   public class TestResult2StringConverter : ConverterBase
   {
      public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         var result = (TestResult)value;
         return result.Test;
      }
   }
}