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

         var xml = XElement.Parse(content);

         var propertyGroups = xml.Elements()
                                 .Where(x => x.IsLocalName("PropertyGroup"))
                                 .ToList();

         var defaults = propertyGroups.Single(x => !x.HasAttributes);

         var buildProfiles = from pg in propertyGroups
                             let condition = pg.Attributes().SingleOrDefault(x => x.IsLocalName("Condition"))
                             where condition != null
                             let parts = condition.Value.Split(new[] { "==" }, StringSplitOptions.None)
                                                  .ElementAt(1)
                                                  .Split('|')
                                                  .Select(x => x.Trim(' ', '\'', '|'))
                                                  .ToArray()
                             select new ProjectFile.BuildProfile
                                    {
                                       Configuration = parts.ElementAt(0),
                                       OutputPath = pg.Elements().Single(x => x.IsLocalName("OutputPath")).Value,
                                       Platform = parts.ElementAt(1)
                                    };

         xml
            .Elements()
            .Where(x => x.Name.LocalName == "ItemGroup")
            .SelectMany(x => x.Elements())
            .Each(x =>
                  {
                     switch (x.Name.LocalName)
                     {
                        case "ProjectReference":
                           projectReferences.Add(
                              Path.GetFullPath(Path.Combine(projectDirectory, x.Attribute("Include").Value)));
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
                   AssemblyName = defaults.Elements().Single(x => x.IsLocalName("AssemblyName")).Value,
                   BuildProfiles = buildProfiles.ToSet(),
                   DefaultConfiguration = defaults.Elements().Single(x => x.IsLocalName("Configuration")).Value,
                   DefaultPlatform = defaults.Elements().Single(x => x.IsLocalName("Platform")).Value,
                   OutputType = defaults.Elements().Single(x => x.IsLocalName("OutputType")).Value,
                   Files = files,
                   ProjectReferences = projectReferences,
                   References = references
                };
      }
   }
}