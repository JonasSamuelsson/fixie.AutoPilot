using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace Fixie.AutoRun
{
   internal static class TestRunner
   {
      public static async Task Execute(string solutionPath, Action<TestResult> callback, CancellationToken token)
      {
         var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
         var fixie = Path.Combine(dir, "Fixie.Console.exe");
         var reportPath = Path.Combine(Path.GetTempPath(), Path.GetFileName(solutionPath) + ".xml");
         var args = GetPaths(solutionPath).Append("--fixie:XUnitXml")
                                          .Append(reportPath);

         var process = Process.Start(new ProcessStartInfo(fixie)
                                          {
                                             Arguments = string.Join(" ", args),
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

         var output = CleanOutput(process.StandardOutput.ReadToEnd());
         if (!string.IsNullOrWhiteSpace(output))
            MessageBox.Show(output, "fixie", MessageBoxButton.OK, MessageBoxImage.Error);

         (from c in XElement.Load(reportPath).Elements().SelectMany(a => a.Elements())
          let cName = c.Attribute("name").Value
          let indexOfLastDot = cName.LastIndexOf('.')
          let @namespace = cName.Substring(0, indexOfLastDot)
          let @class = cName.Substring(indexOfLastDot + 1)
          from t in c.Elements()
          let test = t.Attribute("name").Value.Substring(cName.Length + 1)
          let testStatus = (TestStatus)Enum.Parse(typeof(TestStatus), t.Attribute("result").Value, true)
          select new TestResult
                 {
                    Class = @class,
                    FailReason = t.Element("failure") == null
                                    ? string.Empty
                                    : t.Element("failure").Attribute("exception-type").Value + t.Element("failure").Element("message").Value,
                    Namespace = @namespace,
                    Test = test,
                    Status = testStatus
                 }).Each(callback);
      }

	   private static string CleanOutput(string output)
	   {
	      var buffer = new List<string>();
		   var result = new List<string>();
		  
         foreach (var line in output.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
		   {
			   if (!string.IsNullOrWhiteSpace(line))
		      {
			      buffer.Add(line);
               continue;
            }
			   if (buffer.Any())
			   {
				   result.Add(string.Join(Environment.NewLine, buffer));
				   buffer.Clear();
			   }
         }
		   
         if (buffer.Any())
			   result.Add(string.Join(Environment.NewLine, buffer));

	      return string.Join(string.Format("{0}{0}", Environment.NewLine),
	                         result.Where(x => !x.StartsWith("-----"))
	                               .Where(x => !x.StartsWith("Test '"))
                                  .Where(x => !Regex.IsMatch(x, @"^\w*\d+ passed")));
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