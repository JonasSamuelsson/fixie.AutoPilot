using Fixie.AutoRun.FixieRunner.Contracts;
using Fixie.Execution;
using System;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;

namespace Fixie.AutoRun.FixieRunner
{
   public static class Program
   {
      public static void Main(string[] args)
      {
         try
         {
            var executionResult = new ExecutionResult();

            foreach (var assemblyPath in args.Where(x => !x.StartsWith("--")))
            {
               var lookup = args
                  .Where(x => x.StartsWith("--"))
                  .Select(x => x.Substring(2))
                  .ToLookup(x => x.Substring(0, x.IndexOf(':')), x => x.Substring(x.IndexOf(':') + 1));

               var listener = CreateListener(lookup);

               using (var environment = new ExecutionEnvironment(assemblyPath))
               {
                  executionResult.Add(environment.RunAssembly(new Options(), listener));
               }
            }
         }
         catch (Exception exception)
         {
            Debug.Fail(exception.ToString());
         }
      }

      private static Listener CreateListener(ILookup<string, string> lookup)
      {
         var uri = lookup["uri"].Single();
         return CreateListener(uri);
         //return uri.Equals("console", StringComparison.CurrentCultureIgnoreCase)
         //          ? new ConsoleListener()
         //          : CreateListener(uri);
      }

      private static Listener CreateListener(string uri)
      {
         var binding = new NetNamedPipeBinding();
         var address = new EndpointAddress(uri);
         var factory = new ChannelFactory<IService>(binding, address);
         var proxy = factory.CreateChannel();
         return new NamedPipesListener(proxy);
      }
   }
}
