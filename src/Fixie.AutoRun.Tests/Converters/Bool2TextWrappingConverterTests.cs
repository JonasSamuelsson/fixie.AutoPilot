using System;
using System.Windows;
using Fixie.AutoRun.Converters;
using Shouldly;

namespace Fixie.AutoRun.Tests.Converters
{
	public class Bool2TextWrappingConverterTests
	{
		public void ShouldConvertFalseToNoWrap()
		{
			new Bool2TextWrappingConverter().Convert(false, null, null, null).ShouldBe(TextWrapping.NoWrap);
		}

		public void ShouldConvertTrueToWrap()
		{
			new Bool2TextWrappingConverter().Convert(true, null, null, null).ShouldBe(TextWrapping.Wrap);
		}

		public void ConvertBackShouldThrow()
		{
			Should.Throw<NotImplementedException>(() => new Bool2TextWrappingConverter().ConvertBack(null, null, null, null));
		}
	}
}