using System;
using System.IO;
using System.Linq;

namespace Fixie.AutoRun.VisualStudio
{
   public static class SolutionFileParser
   {
      public static SolutionFile Parse(string content, string path)
      {
         var solutionDirectory = Path.GetDirectoryName(path);

         var acceptedExtensions = new[] { ".csproj", ".fsproj", ".vbproj" };
         return new SolutionFile
                {
                   Path = path,
                   Projects =
                      (from line in content.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                       where line.StartsWith("Project(")
                       let parts = line.Split(new[] { " = ", ", " }, StringSplitOptions.None)
                       where parts.Length == 4
                       let projectPath = parts.ElementAt(2).Trim('"')
                       where acceptedExtensions.Any(x => projectPath.EndsWith(x, StringComparison.CurrentCultureIgnoreCase))
                       select Path.Combine(solutionDirectory, Path.GetFullPath(Path.Combine(solutionDirectory, projectPath)))).ToSet()
                };
      }
   }
}