using System.IO;

namespace Fixie.AutoRun.FixieRunner
{
   public class Program
   {
      public static void Main(string[] args)
      {
         var parser = new CommandLineParser(args);
         foreach (var assemblyPath in parser.AssemblyPaths)
         {
            var assemblyFullPath = Path.GetFullPath(assemblyPath);
            using (var executionEnvironment = new ExecutionEnvironment(assemblyPath))
            {
               var testRunner = executionEnvironment.Create<TestRunner>();
               testRunner.Execute(assemblyFullPath, args);
            }
         }
      }
   }
}
