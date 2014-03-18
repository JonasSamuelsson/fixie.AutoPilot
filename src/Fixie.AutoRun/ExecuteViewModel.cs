using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Fixie.AutoRun
{
   internal class ExecuteViewModel
   {
      private const string MsBuildPath = @"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\msbuild.exe";
      private const string MsBuildArgs = " /p:Configuration=Debug /p:Platform=\"Any CPU\" /v:minimal /nologo /t:rebuild /tv:4.0";

      private CancellationTokenSource _cancellationTokenSource;
      private readonly EventBus _eventBus;
      private readonly FileSystemWatcher _watcher;
      private bool _hasChanges;

      public ExecuteViewModel(EventBus eventBus)
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

         Path = new Observable<string>();
         Output = new Observable<string>();
         IsEnabled = new Observable<bool>();
         IsEnabled.PropertyChanged += delegate { _watcher.EnableRaisingEvents = IsEnabled; };
         IsExecuting = new Observable<bool>();
         HasErrorsOrFailures = new Observable<bool>();

         BackCommand = new RelayCommand(Back);
         PauseCommand = new RelayCommand(() => IsEnabled.Value = false);
         PlayCommand = new RelayCommand(() => IsEnabled.Value = true);
      }

      private void FileSystemChanged(object sender, FileSystemEventArgs e)
      {
         var patterns = new[] { @"\\bin\\debug", @"\\obj\\debug", @".+\.suo$", @"\.user$" };
         if (patterns.Any(x => Regex.IsMatch(e.FullPath, x, RegexOptions.IgnoreCase))) return;
         _hasChanges = true;
      }

      public Observable<string> Path { get; private set; }
      public Observable<string> Output { get; private set; }
      public Observable<bool> IsEnabled { get; private set; }
      public Observable<bool> IsExecuting { get; private set; }
      public ObservableCollection<TestResult> TestResults { get; private set; }
      public Observable<bool> HasErrorsOrFailures { get; private set; }
      public ICommand BackCommand { get; private set; }
      public ICommand PauseCommand { get; private set; }
      public ICommand PlayCommand { get; private set; }

      public void Run(string solutionPath)
      {
         _hasChanges = true;
         _watcher.Path = System.IO.Path.GetDirectoryName(solutionPath);
         IsEnabled.Value = true;
         Path.Value = solutionPath;
         Output.Value = string.Empty;
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
                                                 if (Compile(solutionPath))
                                                    ExecuteTests(solutionPath);
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
         var dir = System.IO.Path.GetDirectoryName(GetType().Assembly.Location);
         var fixie = System.IO.Path.Combine(dir, "Fixie.Console.exe");

         var fixieProcess = Process.Start(new ProcessStartInfo(fixie)
                                          {
                                             Arguments = string.Join(" ", GetPaths(solution)),
                                             CreateNoWindow = true,
                                             RedirectStandardOutput = true,
                                             UseShellExecute = false,
                                             WorkingDirectory = dir
                                          });
         var output = fixieProcess.StandardOutput.ReadToEnd();
         Output.Value += output.TrimEnd();
         HasErrorsOrFailures.Value = Regex.IsMatch(Output, "Test '.+' failed");
         fixieProcess.WaitForExit();
      }

      private bool Compile(string solution)
      {
         Output.Value = string.Format("------ Compiling {0} ------{1}", System.IO.Path.GetFileName(solution), Environment.NewLine);
         var msbuildProcess = Process.Start(new ProcessStartInfo(MsBuildPath)
                                            {
                                               Arguments = solution + MsBuildArgs,
                                               CreateNoWindow = true,
                                               RedirectStandardOutput = true,
                                               UseShellExecute = false
                                            });
         Output.Value += msbuildProcess.StandardOutput.ReadToEnd() + Environment.NewLine;
         msbuildProcess.WaitForExit();
         return msbuildProcess.ExitCode == 0;
      }

      private static string[] GetPaths(string solution)
      {
         var dir = System.IO.Path.GetDirectoryName(solution);
         var separators = new[] { " = ", ", " };
         return File.ReadAllLines(solution)
             .Where(x => x.StartsWith("Project("))
             .Select(x => x.Split(separators, StringSplitOptions.None)[1].Trim('"'))
             .Select(x => System.IO.Path.Combine(dir, x, @"bin\debug", x + ".dll"))
             .Where(x => File.Exists(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(x), "fixie.dll")))
             .ToArray();
      }

      private void Back()
      {
         _cancellationTokenSource.Cancel();
         _watcher.Dispose();
         _eventBus.Publish<ShowLaunchEvent>();
      }
   }
}