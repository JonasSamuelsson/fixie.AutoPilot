using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace Fixie.AutoRun
{
	public class Path2FileNameConverter : IValueConverter
	{
		public static readonly IValueConverter Instance = new Path2FileNameConverter();

		private Path2FileNameConverter()
		{
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return Path.GetFileNameWithoutExtension((string)value);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}