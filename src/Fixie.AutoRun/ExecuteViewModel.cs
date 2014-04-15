using Fixie.AutoRun.Events;
using Fixie.AutoRun.FixieRunner.Contracts;
using Fixie.AutoRun.Infrastructure;
using Fixie.AutoRun.VisualStudio;
using Fixie.AutoRun.Workers;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace Fixie.AutoRun
{
   internal class ExecuteViewModel : IContentViewModel
   {
      private readonly Dispatcher _dispatcher;
      private readonly EventBus _eventBus;
      private readonly CancellationTokenSource _cancellationTokenSource;
      private SolutionSettings _settings;
      private Solution _solution;
      private Action<bool> _onSettingsClosed;

      public ExecuteViewModel(EventBus eventBus)
      {
         _cancellationTokenSource = new CancellationTokenSource();
         _dispatcher = Dispatcher.CurrentDispatcher;
         _eventBus = eventBus;

         Path = new Observable<string>();
         IsEnabled = new Observable<bool>();
         IsExecuting = new Observable<bool>();
         HasChanges = new Observable<bool>();
         TestResults = new ObservableCollection<TestResult>();
         PassCount = new Observable<int>();
         FailCount = new Observable<int>();
         SkipCount = new Observable<int>();

         MsBuild = new MsBuildViewModel();

         BackCommand = new RelayCommand(Exit);
         PauseCommand = new RelayCommand(() => IsEnabled.Value = false);
         PlayCommand = new RelayCommand(() => IsEnabled.Value = true);
         StopCommand = new RelayCommand(() => _cancellationTokenSource.Cancel());
         ShowSettingsCommand = new RelayCommand(ShowSettings);
      }

      public Observable<string> Path { get; private set; }
      public Observable<bool> IsEnabled { get; private set; }
      public Observable<bool> IsExecuting { get; private set; }
      public Observable<bool> HasChanges { get; private set; }
      public MsBuildViewModel MsBuild { get; set; }
      public ObservableCollection<TestResult> TestResults { get; private set; }
      public Observable<int> PassCount { get; private set; }
      public Observable<int> FailCount { get; private set; }
      public Observable<int> SkipCount { get; private set; }
      public ICommand BackCommand { get; private set; }
      public ICommand PauseCommand { get; private set; }
      public ICommand PlayCommand { get; private set; }
      public ICommand StopCommand { get; private set; }
      public ICommand ShowSettingsCommand { get; private set; }

      private void ShowSettings()
      {
         _eventBus.Publish(new ShowSettingsEvent
                           {
                              Configurations = _solution.Configurations.ToList(),
                              Platforms = _solution.Platforms.ToList(),
                              Settings = _settings
                           });
      }

      private async Task CompileAndTest()
      {
         var cancellationTokenSource = new CancellationTokenSource();
         HasChanges.Value = false;
         IsExecuting.Value = true;
         PrepareNewTestRun();
         try
         {
            var configuration = _settings.MsBuild.Configuration;
            var platform = _settings.MsBuild.Platform;
            var compilerParams = new Compiler.Params
                                 {
                                    Args = _settings.MsBuild.Args,
                                    Callback = x => { MsBuild.Output.Value += x + Environment.NewLine; },
                                    CancellationToken = cancellationTokenSource.Token,
                                    Configuration = configuration,
                                    Platform = platform,
                                    SolutionPath = _settings.Path,
                                    Verbosity = _settings.MsBuild.Verbosity
                                 };
            if (!await Compiler.Execute(compilerParams)) return;
            MsBuild.Visible.Value = false;
            var testRunnerParams = new TestRunner.Params
                                   {
                                      Args = _settings.Fixie.Args,
                                      Callback = HandleTestResult,
                                      SolutionPath = _settings.Path,
                                      TestAssemblyPaths = _settings.Projects
                                                                   .Where(x => x.IsTestProject)
                                                                   .Select(x => _solution[x.Path].GetOutputAssemblyPath(configuration, platform))
                                                                   .ToList(),
                                      Token = _cancellationTokenSource.Token
                                   };
            await TestRunner.Execute(testRunnerParams);
         }
         catch (Exception exception)
         {
            App.Error(exception);
         }
         finally
         {
            IsExecuting.Value = false;
         }
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
                               MsBuild.Output.Value = string.Empty;
                               MsBuild.Visible.Value = true;
                               TestResults.Clear();
                               FailCount.Value = 0;
                               PassCount.Value = 0;
                               SkipCount.Value = 0;
                            });
      }

      private void Exit()
      {
         if (_solution != null) _solution.Dispose();
         //_cancellationTokenSource.Cancel();
         _eventBus.Publish<ShowLaunchEvent>();
      }

      public void Run()
      {
         _eventBus.Subscribe<OpenNewSolutionEvent>(OpenNewSolution);
         _eventBus.Subscribe<OpenSolutionEvent>(OpenSolution);
         _eventBus.Subscribe<SettingsAcceptedEvent>(SettingsAccepted);
         _eventBus.Subscribe<SettingsDiscardedEvent>(SettingsDiscarded);

         Task.Factory.StartNew(async () =>
                                     {
                                        while (true)
                                        {
                                           if (IsEnabled && !IsExecuting && HasChanges)
                                              await CompileAndTest();

                                           await Task.Delay(100.Milliseconds());
                                        }
                                        // ReSharper disable once FunctionNeverReturns
                                     },
                               _cancellationTokenSource.Token,
                               TaskCreationOptions.LongRunning,
                               TaskScheduler.Current);
      }

      private void SettingsAccepted(SettingsAcceptedEvent @event)
      {
         SettingsClosed(true);
      }

      private void SettingsDiscarded(SettingsDiscardedEvent @event)
      {
         SettingsClosed(false);
      }

      private void SettingsClosed(bool settingsAccepted)
      {
         if (_onSettingsClosed == null) return;
         _onSettingsClosed(settingsAccepted);
         _onSettingsClosed = null;
      }

      private void OpenSolution(OpenSolutionEvent @event)
      {
         _eventBus.Publish(new ShowContentEvent { ViewModel = this });
         _settings = SolutionSettings.Load(@event.Id);
         _solution = Solution.Load(_settings.Path);
         // TODO handle projects added/removed
         _solution.Changed += delegate { HasChanges.Value = true; };

         _onSettingsClosed = settingsAccepted =>
                             {
                                if (!settingsAccepted) return;
                                HasChanges.Value = true;
                                IsEnabled.Value = true;
                             };

         if (@event.ShowSettings)
         {
            ShowSettings();
         }
         else
         {
            HasChanges.Value = true;
            IsEnabled.Value = true;
         }
      }

      private void OpenNewSolution(OpenNewSolutionEvent @event)
      {
         _eventBus.Publish(new ShowContentEvent { ViewModel = this });
         _solution = Solution.Load(@event.Path);
         _settings = new SolutionSettings
                     {
                        MsBuild =
                        {
                           Configuration = _solution.Configurations.First(),
                           Platform = _solution.Platforms.First()
                        },
                        Path = @event.Path,
                        Projects = _solution.Select(x => new ProjectSettings
                                                         {
                                                            IsTestProject = x.IsTestProject(),
                                                            Path = x.Path
                                                         })
                                            .ToList()
                     };
         _solution.Changed += delegate { HasChanges.Value = true; };
         _onSettingsClosed = settingsAccepted =>
                             {
                                if (settingsAccepted)
                                {
                                   IsEnabled.Value = true;
                                   HasChanges.Value = true;
                                }
                                else
                                {
                                   Exit();
                                }
                             };

         _eventBus.Publish(new ShowSettingsEvent
                           {
                              Configurations = _solution.Configurations,
                              Platforms = _solution.Platforms,
                              Settings = _settings
                           });
      }

      public class MsBuildViewModel
      {
         public MsBuildViewModel()
         {
            Output = new Observable<string>();
            Visible = new Observable<bool>();
         }
         public Observable<string> Output { get; private set; }
         public Observable<bool> Visible { get; private set; }
      }
   }
}