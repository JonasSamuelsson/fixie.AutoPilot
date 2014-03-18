using System;
using System.Collections.ObjectModel;
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
                                                 if (await Compiler.Execute(solutionPath, x => Output.Value += x, _cancellationTokenSource.Token))
                                                    await TestRunner.Execute(solutionPath, x => Output.Value += x, _cancellationTokenSource.Token);
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

      private void Back()
      {
         _cancellationTokenSource.Cancel();
         _watcher.Dispose();
         _eventBus.Publish<ShowLaunchEvent>();
      }
   }
}