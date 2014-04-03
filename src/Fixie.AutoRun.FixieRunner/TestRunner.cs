using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Windows.Forms;
using Fixie.AutoRun.FixieRunner.Contracts;

namespace Fixie.AutoRun.FixieRunner
{
	internal class TestRunner : MarshalByRefObject
	{
		public void Execute(string assemblyFullPath, string[] args)
		{
			try
			{
				var directory = Path.GetDirectoryName(assemblyFullPath);
				AppDomain.CurrentDomain.SetData("APPBASE", directory);
				var parser = new CommandLineParser(args);
				var options = parser.Options;
				var uri = options["uri"].Single();
				var binding = new NetNamedPipeBinding();
				var address = new EndpointAddress(uri);
				var factory = new ChannelFactory<IService>(binding, address);
				var proxy = factory.CreateChannel();
				var listener = new Listener(proxy);
				var runner = new Runner(listener, options);
				var assembly = Assembly.Load(AssemblyName.GetAssemblyName(assemblyFullPath));
				runner.RunAssembly(assembly);
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.ToString(), "FixieRunner");
			}
		}
	}
}