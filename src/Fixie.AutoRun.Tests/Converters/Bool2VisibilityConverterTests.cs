using Fixie.AutoRun.Converters;
using Shouldly;
using System;
using System.Windows;

namespace Fixie.AutoRun.Tests.Converters
{
   public class Bool2VisibilityConverterTests
   {
      public void ShouldConvertFalseToCollapsed()
      {
         new Bool2VisibilityConverter().Convert(false, null, null, null).ShouldBe(Visibility.Collapsed);
      }

      public void ShouldConvertFalseToCustomVisibility()
      {
         new Bool2VisibilityConverter { False = Visibility.Hidden }.Convert(false, null, null, null).ShouldBe(Visibility.Hidden);
      }

      public void ShouldConvertTrueToVisible()
      {
         new Bool2VisibilityConverter().Convert(true, null, null, null).ShouldBe(Visibility.Visible);
      }

      public void ConvertBackShouldThrow()
      {
         Should.Throw<NotImplementedException>(() => new Bool2VisibilityConverter().ConvertBack(null, null, null, null));
      }
   }
}