using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Fixie.AutoRun
{
   public static class TestRunner
   {
      public static async Task Execute(string solutionPath, Action<string> callback, CancellationToken token)
      {
         var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
         var fixie = Path.Combine(dir, "Fixie.Console.exe");

         var process = Process.Start(new ProcessStartInfo(fixie)
                                          {
                                             Arguments = string.Join(" ", GetPaths(solutionPath)),
                                             CreateNoWindow = true,
                                             RedirectStandardOutput = true,
                                             UseShellExecute = false,
                                             WorkingDirectory = dir
                                          });

         while (!process.HasExited)
         {
            if (token.IsCancellationRequested)
            {
               process.Kill();
               return;
            }

            await Task.Delay(50, token);
         }

         callback(process.StandardOutput.ReadToEnd().TrimEnd());
      }

      private static string[] GetPaths(string solution)
      {
         var dir = Path.GetDirectoryName(solution);
         var separators = new[] { " = ", ", " };
         return File.ReadAllLines(solution)
             .Where(x => x.StartsWith("Project("))
             .Select(x => x.Split(separators, StringSplitOptions.None)[1].Trim('"'))
             .Select(x => Path.Combine(dir, x, @"bin\debug", x + ".dll"))
             .Where(x => File.Exists(Path.Combine(Path.GetDirectoryName(x), "fixie.dll")))
             .ToArray();
      }
   }
}