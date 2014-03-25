using System;
using Fixie.AutoRun.Converters;
using Shouldly;

namespace Fixie.AutoRun.Tests.Converters
{
	public class Path2FileNameConverterTests
	{
		public void ShouldConvertPathToFileName()
		{
			new Path2FileNameConverter().Convert(@"c:\directory\file.extension", null, null, null).ShouldBe("file");
		}

		public void ConvertBackShouldThrow()
		{
			Should.Throw<NotImplementedException>(() => new Path2FileNameConverter().ConvertBack(null, null, null, null));
		}
	}
}