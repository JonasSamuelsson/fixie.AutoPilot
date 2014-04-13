using System.Collections.Generic;

namespace Fixie.AutoRun.VisualStudio
{
   public class SolutionFile
   {
      public string Path { get; set; }
      public ISet<string> Projects { get; set; }
      public ISet<string> Configurations { get; set; }
      public ISet<string> Platforms { get; set; }
   }
}