using System;
using System.Collections.Generic;
using System.Linq;

namespace Fixie.AutoRun.VisualStudio
{
   public class ProjectFile : IProject
   {
      public string Path { get; set; }
      public string OutputType { get; set; }
      public string AssemblyName { get; set; }
      public string DefaultConfiguration { get; set; }
      public string DefaultPlatform { get; set; }
      public ISet<BuildProfile> BuildProfiles { get; set; }
      public ISet<string> Files { get; set; }
      public ISet<string> ProjectReferences { get; set; }
      public ISet<string> References { get; set; }

      IReadOnlyCollection<string> IProject.ProjectReferences
      {
         get { return ProjectReferences.ToList(); }
      }

      IReadOnlyCollection<string> IProject.References
      {
         get { return References.ToList(); }
      }

      string IProject.GetOutputAssemblyPath(string configuration, string platform)
      {
         var patterns = new[]
                        {
                           new {configuration, platform},
                           new {configuration, platform = DefaultPlatform},
                           new {configuration = DefaultConfiguration, platform},
                           new {configuration = DefaultConfiguration, platform = DefaultPlatform}
                        };

         foreach (var pattern in patterns)
         {
            foreach (var buildProfile in BuildProfiles)
            {
               if (!pattern.configuration.Equals(buildProfile.Configuration, StringComparison.CurrentCultureIgnoreCase))
                  continue;
               if (!pattern.platform.Equals(buildProfile.Platform, StringComparison.CurrentCultureIgnoreCase))
                  continue;
               var directory = System.IO.Path.GetDirectoryName(Path);
               var filename = string.Format("{0}.{1}", AssemblyName, OutputType == "Library" ? "dll" : "exe");
               return System.IO.Path.Combine(directory, buildProfile.OutputPath, filename);
            }
         }

         throw new InvalidOperationException();
      }

      public class BuildProfile
      {
         public string Configuration { get; set; }
         public string Platform { get; set; }
         public string OutputPath { get; set; }
      }
   }
}