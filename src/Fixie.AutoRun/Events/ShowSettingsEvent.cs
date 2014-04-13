using System.Collections.Generic;

namespace Fixie.AutoRun.Events
{
   public class ShowSettingsEvent
   {
      public SolutionSettings Settings { get; set; }
      public IReadOnlyCollection<string> Configurations { get; set; }
      public IReadOnlyCollection<string> Platforms { get; set; }
   }
}