using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Fixie.AutoRun
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
            _watcher = new FileSystemWatcher
                       {
                           EnableRaisingEvents = false,
                           IncludeSubdirectories = true
                       };
            _watcher.Changed += FileSystemChanged;
            _watcher.Created += FileSystemChanged;
            _watcher.Deleted += FileSystemChanged;
            _watcher.Renamed += FileSystemChanged;
            TestResults = new ObservableCollection<TestResult>();

            _eventBus.Handle<StartEvent>(HandleStartEvent);

            Visible = new Observable<bool>();
            Text = new Observable<string>();
            IsExecuting = new Observable<bool>();
            HasErrorsOrFailures = new Observable<bool>();
        }

        private void FileSystemChanged(object sender, FileSystemEventArgs e)
        {
            var patterns = new[] { @"\\bin\\debug", @"\\obj\\debug", @".+\.suo$", @"\.user$" };
            if (patterns.Any(x => Regex.IsMatch(e.FullPath, x, RegexOptions.IgnoreCase))) return;
            _hasChanges = true;
        }

        public Observable<bool> Visible { get; private set; }
        public Observable<string> Text { get; private set; }
        public Observable<bool> IsExecuting { get; private set; }
        public ObservableCollection<TestResult> TestResults { get; private set; }
        public Observable<bool> HasErrorsOrFailures { get; private set; }

        private void HandleStartEvent(StartEvent @event)
        {
            _hasChanges = true;
            var solution = @event.Solution;
            _watcher.Path = Path.GetDirectoryName(solution);
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
                                                    HasErrorsOrFailures.Value = false;
                                                    try
                                                    {
                                                        if (Compile(solution))
                                                            ExecuteTests(solution);
                                                        else
                                                            HasErrorsOrFailures.Value = true;
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
            var output = fixieProcess.StandardOutput.ReadToEnd();
            Text.Value += output.TrimEnd();
            HasErrorsOrFailures.Value = Regex.IsMatch(Text, "Test '.+' failed");
            fixieProcess.WaitForExit();
        }

        private bool Compile(string solution)
        {
            Text.Value = string.Format("------ Compiling {0} ------{1}", Path.GetFileName(solution), Environment.NewLine);
            var msbuildProcess = Process.Start(new ProcessStartInfo(msbuild)
                                               {
                                                   Arguments = solution + msbuildArgs,
                                                   CreateNoWindow = true,
                                                   RedirectStandardOutput = true,
                                                   UseShellExecute = false
                                               });
            Text.Value += msbuildProcess.StandardOutput.ReadToEnd() + Environment.NewLine;
            msbuildProcess.WaitForExit();
            return msbuildProcess.ExitCode == 0;
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