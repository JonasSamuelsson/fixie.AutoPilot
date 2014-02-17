using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

		private void HandleStartEvent(StartEvent @event)
		{
			_hasChanges = true;
			_watcher.Path = Path.GetDirectoryName(@event.Solution);
			_watcher.EnableRaisingEvents = true;
			Text.Value = string.Empty;
			Visible.Value = true;
			_cancellationTokenSource = new CancellationTokenSource();

			Task.Factory.StartNew(async () =>
												 {
													 while (true)
													 {
														 if (_hasChanges)
														 {
															 _hasChanges = false;
															 IsExecuting.Value = true;
															 var msbuildProcess = Process.Start(new ProcessStartInfo(msbuild)
																											{
																												Arguments = @event.Solution + msbuildArgs,
																												CreateNoWindow = true,
																												RedirectStandardOutput = true,
																												UseShellExecute = false
																											});
															 Text.Value = msbuildProcess.StandardOutput.ReadToEnd();
															 msbuildProcess.WaitForExit();
															 if (msbuildProcess.ExitCode == 0)
															 {
																 var dir = Path.GetDirectoryName(GetType().Assembly.Location);
																 var fixie = Path.Combine(dir, "Fixie.Console.exe");

																 var fixieProcess = Process.Start(new ProcessStartInfo(fixie)
																											 {
																												 Arguments = string.Join(" ", GetPaths(@event.Solution)),
																												 CreateNoWindow = true,
																												 RedirectStandardOutput = true,
																												 UseShellExecute = false,
																												 WorkingDirectory = dir
																											 });
																 Text.Value += fixieProcess.StandardOutput.ReadToEnd();
																 fixieProcess.WaitForExit();
															 }
														 }

														 IsExecuting.Value = false;
														 if (_hasChanges) continue;
														 await Task.Delay(TimeSpan.FromSeconds(10));
													 }
												 },
				 _cancellationTokenSource.Token,
				 TaskCreationOptions.LongRunning,
				 TaskScheduler.Current);
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