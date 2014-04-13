using System.Collections.Generic;

namespace Fixie.AutoRun.VisualStudio
{
   public interface IProject
   {
      string Path { get; }
      IReadOnlyCollection<string> ProjectReferences { get; }
      IReadOnlyCollection<string> References { get; }
      string GetOutputAssemblyPath(string configuration, string platform);
   }
}