using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Fixie.AutoPilot
{
	public class ContentViewModel
	{
		private const string msbuild = @"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\msbuild.exe";
		private const string msbuildArgs = " /p:Configuration=Debug /p:Platform=\"Any CPU\" /v:minimal /nologo /t:rebuild /tv:4.0";

		private CancellationTokenSource _cancellationTokenSource;
		private readonly EventBus _eventBus;
		private readonly FileSystemWatcher _watcher;
		private bool _hasChanges;

		public ContentViewModel(EventBus eventBus)
		{
			_eventBus = eventBus;
			_watcher = new FileSystemWatcher { EnableRaisingEvents = false };
			_watcher.Changed += FileSystemChanged;
			_watcher.Created += FileSystemChanged;
			_watcher.Deleted += FileSystemChanged;
			_watcher.Renamed += FileSystemChanged;
			TestResults = new ObservableCollection<TestResult>();

			_eventBus.Handle<StartEvent>(HandleStartEvent);

			Visible = new Observable<bool>();
			Text = new Observable<string>();
			IsExecuting = new Observable<bool>();
		}

		private void FileSystemChanged(object sender, FileSystemEventArgs e)
		{
			if (e.FullPath.IndexOf(@"\bin\debug\", StringComparison.CurrentCultureIgnoreCase) != -1) return;
			_hasChanges = true;
		}

		public Observable<bool> Visible { get; private set; }
		public Observable<string> Text { get; private set; }
		public Observable<bool> IsExecuting { get; private set; }
		public ObservableCollection<TestResult> TestResults { get; private set; }

		private void HandleStartEvent(StartEvent @event)
		{
			_hasChanges = true;
			var solution = @event.Solution;
			_watcher.Path = Path.GetDirectoryName(solution);
			_watcher.EnableRaisingEvents = true;
			Text.Value = string.Empty;
			Visible.Value = true;
			_cancellationTokenSource = new CancellationTokenSource();

			var dispatcher = Dispatcher.CurrentDispatcher;

			Task.Factory.StartNew(async () =>
												 {
													 while (true)
													 {
														 if (_hasChanges)
														 {
															 _hasChanges = false;
															 IsExecuting.Value = true;
															 try
															 {
																 if (Compile(solution))
																	 ExecuteTests(solution);
															 }
															 catch (Exception exception)
															 {
																 App.Error(exception);
															 }
															 IsExecuting.Value = false;
															 if (_hasChanges) continue;
														 }

														 await Task.Delay(TimeSpan.FromSeconds(1));
													 }
												 },
										 _cancellationTokenSource.Token,
										 TaskCreationOptions.LongRunning,
										 TaskScheduler.Current);
		}

		private void ExecuteTests(string solution)
		{
			var dir = Path.GetDirectoryName(GetType().Assembly.Location);
			var fixie = Path.Combine(dir, "Fixie.Console.exe");

			var fixieProcess = Process.Start(new ProcessStartInfo(fixie)
														{
															Arguments = string.Join(" ", GetPaths(solution)),
															CreateNoWindow = true,
															RedirectStandardOutput = true,
															UseShellExecute = false,
															WorkingDirectory = dir
														});
			var output = fixieProcess.StandardOutput.ReadToEnd()
											 .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
											 .Select(x => x.TrimStart('\n'));

			var list = new List<TestResult>();
			TestResult result = null;
			foreach (var line in output)
			{
				if (line.StartsWith("-----"))
					continue;
				if (Regex.IsMatch(line, @"\d+ passed, \d+ failed", RegexOptions.IgnoreCase))
					continue;

				if (line.StartsWith("Test "))
				{
					result = new TestResult { Name = new Observable<string>(line), Info = new Observable<string>() };
					list.Add(result);
				}
				else
					result.Info.Value += line;
			}

			Application.Current.Dispatcher.Invoke(() =>
			                                      {
				                                      TestResults.Clear();
				                                      foreach (var item in list)
					                                      TestResults.Add(item);
			                                      });

			fixieProcess.WaitForExit();
		}

		private bool Compile(string solution)
		{
			var msbuildProcess = Process.Start(new ProcessStartInfo(msbuild)
														  {
															  Arguments = solution + msbuildArgs,
															  CreateNoWindow = true,
															  RedirectStandardOutput = true,
															  UseShellExecute = false
														  });
			Text.Value = msbuildProcess.StandardOutput.ReadToEnd();
			msbuildProcess.WaitForExit();
			var buildSucceeded = msbuildProcess.ExitCode == 0;
			return buildSucceeded;
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

	public class TestResult
	{
		public Observable<string> Name { get; set; }
		public Observable<string> Info { get; set; }
	}
}