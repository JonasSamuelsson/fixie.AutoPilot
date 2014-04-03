using System;
using System.Collections.Generic;
using System.Linq;

namespace Fixie.AutoRun.VisualStudio
{
   public class SolutionChangedEventArgs
   {
      public static readonly SolutionChangedEventArgs Empty = new SolutionChangedEventArgs();

      public SolutionChangedEventArgs()
      {
         AddedProjects = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
         ChangedProjects = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
         DeletedProjects = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
         RenamedProjects = new HashSet<Rename>();
      }

      public SolutionChangedEventArgs(IEnumerable<string> addedProjects, IEnumerable<string> changedProjects, IEnumerable<string> deletedProjects, IEnumerable<Rename> renamedProjects)
      {
         AddedProjects = (addedProjects ?? new string[] { }).ToSet(StringComparer.CurrentCultureIgnoreCase);
         ChangedProjects = (changedProjects ?? new string[] { }).ToSet(StringComparer.CurrentCultureIgnoreCase);
         DeletedProjects = (deletedProjects ?? new string[] { }).ToSet(StringComparer.CurrentCultureIgnoreCase);
         RenamedProjects = (renamedProjects ?? new Rename[] { }).ToSet();
      }

      public ISet<string> AddedProjects { get; private set; }
      public ISet<string> ChangedProjects { get; private set; }
      public ISet<string> DeletedProjects { get; private set; }
      public ISet<Rename> RenamedProjects { get; private set; }

      public bool IsEmpty
      {
         get { return !AddedProjects.Any() && !ChangedProjects.Any() && !DeletedProjects.Any() && !RenamedProjects.Any(); }
      }

      public class Rename
      {
         public Rename(string oldPath, string newPath)
         {
            OldPath = oldPath;
            NewPath = newPath;
         }

         public string OldPath { get; private set; }
         public string NewPath { get; private set; }
      }
   }
}