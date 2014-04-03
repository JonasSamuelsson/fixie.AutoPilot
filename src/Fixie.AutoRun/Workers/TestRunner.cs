using Fixie.AutoRun.FixieRunner.Contracts;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace Fixie.AutoRun.Workers
{
   internal static class TestRunner
   {
      public static async Task Execute(string solutionPath, Action<TestResult> callback, CancellationToken token)
      {
         const string uri = "net.pipe://localhost/fixie.autorun";
         var address = Guid.NewGuid().ToString();
         var service = new Service(callback);
         var serviceHost = new ServiceHost(service, new Uri(uri));
         serviceHost.AddServiceEndpoint(typeof(IService), new NetNamedPipeBinding(), address);
         serviceHost.Open();

         var executingAssemblyPath = Assembly.GetExecutingAssembly().Location;
         var directory = Path.GetDirectoryName(executingAssemblyPath);
         var fixieRunnerPath = typeof(IService).Assembly.Location;
         var args = GetPaths(solutionPath).Append("--uri")
                                          .Append(string.Format("{0}/{1}", uri, address))
                                          .ToArray();


         var process = Process.Start(new ProcessStartInfo(fixieRunnerPath)
                                          {
                                             Arguments = string.Join(" ", args),
                                             CreateNoWindow = true,
                                             RedirectStandardOutput = true,
                                             UseShellExecute = false,
                                             WorkingDirectory = directory
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
      }

      private static string[] GetPaths(string solution)
      {
         var dir = Path.GetDirectoryName(solution);
         var separators = new[] { " = ", ", " };
         return File.ReadAllLines(solution)
             .Where(x => x.StartsWith("Project("))
             .Select(x => x.Split(separators, StringSplitOptions.None)[1].Trim('"'))
             .SelectMany(x => new[] { ".dll", ".exe" }.Select(y => Path.Combine(dir, x, @"bin\debug", x + y)).Where(File.Exists))
             .Where(x => File.Exists(Path.Combine(Path.GetDirectoryName(x), "fixie.dll")))
             .ToArray();
      }

      [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
      private class Service : IService
      {
         private readonly Action<TestResult> _callback;

         public Service(Action<TestResult> callback)
         {
            _callback = callback;
         }

         public void TestCompleted(TestResult testResult)
         {
            _callback(testResult);
         }

         public void Error(string type, string message, string stackTrace)
         {
            throw new NotImplementedException();
         }
      }
   }
}