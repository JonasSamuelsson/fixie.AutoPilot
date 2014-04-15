using Fixie.AutoRun.VisualStudio;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Fixie.AutoRun.Workers
{
   public static class Compiler
   {
      private const string MsBuildPath = @"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\msbuild.exe";
      private const string MsBuildArgs = "/p:Configuration=\"{0}\" /p:Platform=\"{1}\" /v:{2} /nologo /t:rebuild /tv:4.0 /m /nr:false";

      public static async Task<bool> Execute(Params @params)
      {
         var arguments = string.Join(" ",
                                     string.Format("\"{0}\"", @params.SolutionPath),
                                     @params.Args,
                                     string.Format(MsBuildArgs,
                                                   @params.Configuration,
                                                   @params.Platform,
                                                   @params.Verbosity));
         var process = new Process
                       {
                          StartInfo = new ProcessStartInfo(MsBuildPath)
                                      {
                                         Arguments = arguments,
                                         CreateNoWindow = true,
                                         RedirectStandardOutput = true,
                                         UseShellExecute = false
                                      }
                       };
         process.OutputDataReceived += (sender, args) => @params.Callback(args.Data);
         process.Start();
         process.BeginOutputReadLine();
         while (!process.HasExited)
         {
            if (@params.CancellationToken.IsCancellationRequested)
            {
               process.Kill();
               return false;
            }

            await Task.Delay(50, @params.CancellationToken);
         }

         return process.ExitCode == 0;
      }

      public class Params
      {
         public string Args { get; set; }
         public string Configuration { get; set; }
         public string Platform { get; set; }
         public MsBuildVerbosity Verbosity { get; set; }
         public string SolutionPath { get; set; }
         public Action<string> Callback { get; set; }
         public CancellationToken CancellationToken { get; set; }
      }
   }
}