using System;

namespace Fixie.AutoRun.FileSystem
{
   public class RenamedEventArgs : EventArgs
   {
      public RenamedEventArgs(string oldPath, string newPath)
      {
         OldPath = oldPath;
         NewPath = newPath;
      }

      public string OldPath { get; private set; }
      public string NewPath { get; private set; }
   }
}