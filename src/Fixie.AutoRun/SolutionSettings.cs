using System.Linq;
using Fixie.AutoRun.VisualStudio;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;

namespace Fixie.AutoRun
{
   public class SolutionSettings
   {
      public SolutionSettings()
      {
         Id = DateTime.Now.Ticks;
         Fixie = new FixieSettings
                 {
                    Args = string.Empty
                 };
         MsBuild = new MsBuildSettings
                   {
                      Args = string.Empty,
                      Configuration = string.Empty,
                      Platform = string.Empty,
                      Verbosity = MsBuildVerbosity.Minimal
                   };
         Projects = new List<ProjectSettings>();
      }

      public long Id { get; set; }
      public string Path { get; set; }
      public List<ProjectSettings> Projects { get; set; }
      public MsBuildSettings MsBuild { get; set; }
      public FixieSettings Fixie { get; set; }

      public static SolutionSettings Load(long id)
      {
         var path = System.IO.Path.Combine(Constants.AppDataDirectory, id + ".settings");
         var content = File.ReadAllText(path);
         return JsonConvert.DeserializeObject<SolutionSettings>(content);
      }

      public static IEnumerable<HistoryItem> Load()
      {
         return Directory.GetFiles(Constants.AppDataDirectory, "*.settings")
                         .Select(File.ReadAllText)
                         .Select(JsonConvert.DeserializeObject<HistoryItem>)
                         .ToList();
      }

      public void Save()
      {
         var path = System.IO.Path.Combine(Constants.AppDataDirectory, Id + ".settings");
         var content = JsonConvert.SerializeObject(this, new JsonSerializerSettings
                                                         {
                                                            Formatting = Formatting.Indented,
                                                            Converters = { new StringEnumConverter() }
                                                         });
         File.WriteAllText(path, content);
      }

      public class HistoryItem
      {
         public long Id { get; set; }
         public string Path { get; set; }
      }
   }

   public class ProjectSettings
   {
      public string Path { get; set; }
      public bool IsTestProject { get; set; }
   }

   public class MsBuildSettings
   {
      public string Args { get; set; }
      public string Configuration { get; set; }
      public string Platform { get; set; }
      public MsBuildVerbosity Verbosity { get; set; }
   }

   public class FixieSettings
   {
      public string Args { get; set; }
   }
}