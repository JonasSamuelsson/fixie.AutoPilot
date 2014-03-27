using System;
using System.IO;

namespace Fixie.AutoRun
{
   public class SolutionWatcher
   {
      private readonly Func<string, string> _fileReader;

      public SolutionWatcher(string solutionPath, Func<string, string> fileReader, IFileSystemWatcher fileSystemWatcher)
      {
         _fileReader = fileReader ?? File.ReadAllText;
      }

      public void Start() { }
   }

   public interface IFileSystemWatcher
   {
      string Directory { get; set; }
      //event Changed 
   }

   //public ChangedEventArgs
}