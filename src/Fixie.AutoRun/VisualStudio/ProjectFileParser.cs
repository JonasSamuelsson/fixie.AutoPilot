using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Fixie.AutoRun.VisualStudio
{
   public static class ProjectFileParser
   {
      public static ProjectFile Parse(string content, string path)
      {
         var projectDirectory = Path.GetDirectoryName(path);

         var files = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
         var projectReferences = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
         var references = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);

         XElement.Parse(content)
                 .Elements()
                 .Where(x => x.Name.LocalName == "ItemGroup")
                 .SelectMany(x => x.Elements())
                 .Each(x =>
                       {
                          switch (x.Name.LocalName)
                          {
                             case "ProjectReference":
                                projectReferences.Add(Path.GetFullPath(Path.Combine(projectDirectory, x.Attribute("Include").Value)));
                                break;

                             case "Reference":
                                var hintPathXml = x.Elements().FirstOrDefault(y => y.Name.LocalName == "HintPath");
                                if (hintPathXml == null) break;
                                references.Add(Path.GetFullPath(Path.Combine(projectDirectory, hintPathXml.Value)));
                                break;

                             default:
                                var includeXml = x.Attribute("Include");
                                if (includeXml == null) break;
                                files.Add(Path.GetFullPath(Path.Combine(projectDirectory, includeXml.Value)));
                                break;
                          }
                       });

         return new ProjectFile
                {
                   Path = path,
                   Files = files,
                   ProjectReferences = projectReferences,
                   References = references
                };
      }
   }
}