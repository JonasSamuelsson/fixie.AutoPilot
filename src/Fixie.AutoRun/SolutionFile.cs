using System.Collections.Generic;

namespace Fixie.AutoRun
{
   public class SolutionFile
   {
      public string Path { get; set; }
      public ISet<string> Projects { get; set; }
   }
}