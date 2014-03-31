using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace Fixie.AutoRun
{
   internal class ExecuteViewModel
   {
      private readonly Dispatcher _dispatcher;
      private readonly EventBus _eventBus;
      private bool _hasChanges;
      private CancellationTokenSource _cancellationTokenSource;
      private Solution _solution;

      public ExecuteViewModel(EventBus eventBus)
      {
         _dispatcher = Dispatcher.CurrentDispatcher;
         _eventBus = eventBus;

         Path = new Observable<string>();
         Output = new Observable<string>();
         IsEnabled = new Observable<bool>();
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
         _solution = Solution.Load(solutionPath);
         _solution.Changed += delegate { _hasChanges = true; };
         _hasChanges = true;
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
         if (_solution != null) _solution.Dispose();
         _cancellationTokenSource.Cancel();
         _eventBus.Publish<ShowLaunchEvent>();
      }
   }

   internal enum TestStatus
   {
      Pass, Fail, Skip
   }
}