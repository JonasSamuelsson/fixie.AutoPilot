using System.Collections.Generic;

namespace Fixie.AutoRun
{
   public class Solution
   {
      public string Name { get; private set; }
      public string Path { get; private set; }
      public IReadOnlyList<Project> Projects { get; private set; }
   }

   public class Project
   {
      public string Name { get; private set; }
      public string Path { get; private set; }
      public IReadOnlyList<string> References { get; private set; }
   }
}