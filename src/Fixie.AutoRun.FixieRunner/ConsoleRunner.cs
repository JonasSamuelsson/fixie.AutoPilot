using Fixie.AutoRun.FixieRunner.Contracts;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Windows.Forms;

namespace Fixie.AutoRun.FixieRunner
{
   internal class ConsoleRunner : MarshalByRefObject
   {
      public void RunAssembly(string assemblyFullPath, string[] args)
      {
         try
         {
            SetApplicationBaseDirectory(assemblyFullPath);

            var options = new CommandLineParser(args).Options;
            var listener = CreateListener(options);
            var runner = new Runner(listener, options);
            var assembly = Assembly.Load(AssemblyName.GetAssemblyName(assemblyFullPath));
            runner.RunAssembly(assembly);
         }
         catch (Exception exception)
         {
            MessageBox.Show(exception.ToString(), "FixieRunner");
         }
      }

      private static void SetApplicationBaseDirectory(string assemblyFullPath)
      {
         var directory = Path.GetDirectoryName(assemblyFullPath);
         AppDomain.CurrentDomain.SetData("APPBASE", directory);
      }

      private static Listener CreateListener(ILookup<string, string> options)
      {
         var uri = options["uri"].Single();
         var binding = new NetNamedPipeBinding();
         var address = new EndpointAddress(uri);
         var factory = new ChannelFactory<IService>(binding, address);
         var proxy = factory.CreateChannel();
         var listener = new Listener(proxy);
         return listener;
      }
   }
}