using System.Collections.Generic;

namespace Fixie.AutoRun
{
   public class ProjectFile
   {
      public string Path { get; set; }
      public ISet<string> Files { get; set; }
      public ISet<string> ProjectReferences { get; set; }
      public ISet<string> References { get; set; }
   }
}