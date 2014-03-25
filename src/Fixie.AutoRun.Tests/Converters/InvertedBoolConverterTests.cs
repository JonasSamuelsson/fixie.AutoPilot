using System;
using System.Globalization;
using System.Windows;
using Fixie.AutoRun.Converters;
using Shouldly;

namespace Fixie.AutoRun.Tests.Converters
{
   public class InvertedBoolConverterTests
   {
      public void FalseShouldConvertToTrue()
      {
         new InvertedBoolConverter().Convert(false, null, null, null).ShouldBe(true);
      }

      public void FalseShouldConvertBackToTrue()
      {
         new InvertedBoolConverter().ConvertBack(false, null, null, null).ShouldBe(true);
      }

      public void TrueShouldConvertToFalse()
      {
         new InvertedBoolConverter().Convert(false, null, null, null).ShouldBe(true);
      }

      public void TrueShouldConvertBackToFalse()
      {
         new InvertedBoolConverter().ConvertBack(false, null, null, null).ShouldBe(true);
      }

      public void ShouldConvertUsingInnerConverter()
      {
         new InvertedBoolConverter
         {
            Converter = new Bool2StringConverter()
         }.Convert(false, null, null, null).ShouldBe("True");
      }

      public void ShouldConvertBackUsingInnerConverter()
      {
         new InvertedBoolConverter
         {
            Converter = new Bool2StringConverter()
         }.ConvertBack("true", null, null, null).ShouldBe(false);
      }

      private class Bool2StringConverter : ConverterBase
      {
         public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
         {
            return value.ToString();
         }

         public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
         {
            return bool.Parse((string)value);
         }
      }
   }
}