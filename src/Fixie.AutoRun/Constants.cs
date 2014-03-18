using System;
using System.IO;

namespace Fixie.AutoRun
{
   public static class Constants
   {
      public static string ApplicationName
      {
         get { return "fixie.AutoRun"; }
      }

      public static string AppDataDirectory
      {
         get
         {
            var root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(root, ApplicationName);
         }
      }
   }
}