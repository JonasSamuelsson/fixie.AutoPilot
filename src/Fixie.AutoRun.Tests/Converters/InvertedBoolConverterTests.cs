using Fixie.AutoRun.Converters;
using Shouldly;

namespace Fixie.AutoRun.Tests.Converters
{
   public class InvertedBoolConverterTests
   {
      public void ShouldConvertFalseToTrue()
      {
         InvertedBoolConverter.Instance.Convert(false, null, null, null).ShouldBe(true);
      }

      public void ShouldConvertTrueToFalse()
      {
         InvertedBoolConverter.Instance.Convert(false, null, null, null).ShouldBe(true);
      }
   }
}