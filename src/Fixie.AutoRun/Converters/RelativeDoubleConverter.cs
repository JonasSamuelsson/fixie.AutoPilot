using System;
using System.Globalization;

namespace Fixie.AutoRun.Converters
{
   public class RelativeDoubleConverter:ConverterBase
   {
	   private readonly double _difference;

	   public RelativeDoubleConverter(double difference)
      {
	      _difference = difference;
      }

	   public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	   {
		   return (double)value + _difference;
	   }
   }
}