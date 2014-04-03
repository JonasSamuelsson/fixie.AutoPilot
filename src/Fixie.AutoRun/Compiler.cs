using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Fixie.AutoRun
{
   public static class Compiler
   {
      private const string MsBuildPath = @"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\msbuild.exe";
      private const string MsBuildArgs = " /p:Configuration=Debug /p:Platform=\"Any CPU\" /v:minimal /nologo /t:rebuild /tv:4.0 /m /nr:false";

      public static async Task<bool> Execute(string solutionPath, Action<string> callback, CancellationToken token)
      {
         callback(string.Format("------ Compiling {0} ------{1}", System.IO.Path.GetFileName(solutionPath), Environment.NewLine));
         var process = Process.Start(new ProcessStartInfo(MsBuildPath)
                                            {
                                               Arguments = solutionPath + MsBuildArgs,
                                               CreateNoWindow = true,
                                               RedirectStandardOutput = true,
                                               UseShellExecute = false
                                            });
         while (!process.HasExited)
         {
            if (token.IsCancellationRequested)
            {
               process.Kill();
               return false;
            }

            await Task.Delay(50, token);
         }

         callback(process.StandardOutput.ReadToEnd() + Environment.NewLine);
         return process.ExitCode == 0;
      }
   }
}