using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Fixie.AutoRun
{
   internal class ExecuteViewModel
   {
      private readonly Dispatcher _dispatcher;
      private readonly EventBus _eventBus;
      private readonly FileSystemWatcher _watcher;
      private bool _hasChanges;
      private CancellationTokenSource _cancellationTokenSource;

      public ExecuteViewModel(EventBus eventBus)
      {
         _dispatcher = Dispatcher.CurrentDispatcher;
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

         Path = new Observable<string>();
         Output = new Observable<string>();
         IsEnabled = new Observable<bool>();
         IsEnabled.PropertyChanged += delegate { _watcher.EnableRaisingEvents = IsEnabled; };
         IsExecuting = new Observable<bool>();
         HasErrorsOrFailures = new Observable<bool>();
         TestResults = new ObservableCollection<TestResult>();
         PassCount = new Observable<int>();
         FailCount = new Observable<int>();
         SkipCount = new Observable<int>();

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
      public Observable<bool> HasErrorsOrFailures { get; private set; }
      public ObservableCollection<TestResult> TestResults { get; private set; }
      public Observable<int> PassCount { get; private set; }
      public Observable<int> FailCount { get; private set; }
      public Observable<int> SkipCount { get; private set; }
      public ICommand BackCommand { get; private set; }
      public ICommand PauseCommand { get; private set; }
      public ICommand PlayCommand { get; private set; }

      public void Run(string solutionPath)
      {
         _hasChanges = true;
         _watcher.Path = System.IO.Path.GetDirectoryName(solutionPath);
         IsEnabled.Value = true;
         Path.Value = solutionPath;
         _cancellationTokenSource = new CancellationTokenSource();

          Task.Factory.StartNew(async () =>
                                     {
                                        while (true)
                                        {
                                           if (_hasChanges)
                                           {
                                              _hasChanges = false;
                                              IsExecuting.Value = true;
                                              PrepareNewTestRun();
                                              HasErrorsOrFailures.Value = false;
                                              try
                                              {
                                                 if (await Compiler.Execute(solutionPath, x => Output.Value += x, _cancellationTokenSource.Token))
                                                    await TestRunner.Execute(solutionPath, HandleTestResult, _cancellationTokenSource.Token);
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

       private void HandleTestResult(TestResult result)
       {
          if (result.Status == TestStatus.Pass)
          {
             PassCount.Value++;
             return;
          }

          if (result.Status == TestStatus.Fail) FailCount.Value++;
          if (result.Status == TestStatus.Skip) SkipCount.Value++;
           _dispatcher.Invoke(() => TestResults.Add(result));
       }

       private void PrepareNewTestRun()
       {
          _dispatcher.Invoke(() =>
                             {
                                Output.Value = string.Empty;
                                TestResults.Clear();
                                FailCount.Value = 0;
                                PassCount.Value = 0;
                                SkipCount.Value = 0;
                             });
       }

       private void Back()
      {
         _cancellationTokenSource.Cancel();
         _watcher.Dispose();
         _eventBus.Publish<ShowLaunchEvent>();
      }
   }

    internal enum TestStatus
   {
      Pass, Fail, Skip
   }
}