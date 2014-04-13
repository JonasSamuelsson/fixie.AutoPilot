using Fixie.AutoRun.FixieRunner.Contracts;
using System;
using System.Collections.Generic;
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
      public class Params
      {
         public string SolutionPath { get; set; }
         public IReadOnlyCollection<string> TestAssemblyPaths { get; set; }
         public Action<TestResult> Callback { get; set; }
         public CancellationToken Token { get; set; }
      }

      public static async Task Execute(Params @params)
      {
         const string uri = "net.pipe://localhost/fixie.autorun";
         var address = Guid.NewGuid().ToString();
         var service = new Service(@params.Callback);
         var serviceHost = new ServiceHost(service, new Uri(uri));
         serviceHost.AddServiceEndpoint(typeof(IService), new NetNamedPipeBinding(), address);
         serviceHost.Open();

         var executingAssemblyPath = Assembly.GetExecutingAssembly().Location;
         var directory = Path.GetDirectoryName(executingAssemblyPath);
         var fixieRunnerPath = typeof(IService).Assembly.Location;
         var args = @params.TestAssemblyPaths
                           .Append("--uri")
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
            if (@params.Token.IsCancellationRequested)
            {
               process.Kill();
               return;
            }

            await Task.Delay(50, @params.Token);
         }
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