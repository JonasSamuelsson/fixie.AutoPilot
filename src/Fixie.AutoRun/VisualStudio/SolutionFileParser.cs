using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Fixie.AutoRun.VisualStudio
{
   public static class SolutionFileParser
   {
      public static SolutionFile Parse(string content, string path)
      {
         var solutionDirectory = Path.GetDirectoryName(path);

         var configurations = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
         var platforms = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
         var projects = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
         var configurationPlatforms = false;

         foreach (var line in content.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()))
         {
            if (line == "GlobalSection(SolutionConfigurationPlatforms) = preSolution")
            {
               configurationPlatforms = true;
               continue;
            }

            if (line == "EndGlobalSection")
            {
               configurationPlatforms = false;
               continue;
            }

            if (configurationPlatforms)
            {
               var parts = line.Split(new[] { " = " }, StringSplitOptions.None).First()
                               .Split('|')
                               .ToArray();
               configurations.Add(parts.ElementAt(0));
               platforms.Add(parts.ElementAt(1));
               continue;
            }

            if (line.StartsWith("Project("))
            {
               var acceptedExtensions = new[] { ".csproj", ".fsproj", ".vbproj" };
               var parts = line.Split(new[] { " = ", ", " }, StringSplitOptions.None);
               var projectPath = parts.ElementAt(2).Trim('"');
               if (!acceptedExtensions.Any(x => projectPath.EndsWith(x, StringComparison.CurrentCultureIgnoreCase))) continue;
               projects.Add(Path.Combine(solutionDirectory, Path.GetFullPath(Path.Combine(solutionDirectory, projectPath))));
            }
         }

         return new SolutionFile
                {
                   Configurations = configurations,
                   Path = path,
                   Platforms = platforms,
                   Projects = projects
                };
      }
   }
}