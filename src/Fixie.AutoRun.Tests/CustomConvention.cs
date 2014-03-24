using Fixie.Conventions;
using System.Linq;

namespace Fixie.AutoRun.Tests
{
   class CustomConvention : Convention
   {
      public CustomConvention()
      {
         var suffixes = new[] { "Test", "Tests" };
         Classes.Where(x => suffixes.Any(y => x.Name.EndsWith(y)));
         Methods.Where(x => x.IsPublic && x.IsVoid());
      }
   }
}
