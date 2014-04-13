using System;
using System.Linq;
using System.Windows.Markup;

namespace Fixie.AutoRun
{
   public class Enum2ItemsSource : MarkupExtension
   {
      private readonly Type _type;

      public Enum2ItemsSource(Type type)
      {
         _type = type;
      }

      public override object ProvideValue(IServiceProvider serviceProvider)
      {
         return Enum.GetValues(_type)
                    .Cast<object>();
      }
   }
}