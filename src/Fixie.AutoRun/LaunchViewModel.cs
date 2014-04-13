using System;
using System.Linq;
using Fixie.AutoRun.Events;
using Fixie.AutoRun.Infrastructure;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Forms;
using System.Windows.Input;

namespace Fixie.AutoRun
{
   class LaunchViewModel : IContentViewModel
   {
      private readonly EventBus _eventBus;

      public LaunchViewModel(EventBus eventBus)
      {
         _eventBus = eventBus;

         Path = new Observable<string>();
         History = new ObservableCollection<SolutionSettings.HistoryItem>();
         BrowseCommand = new RelayCommand(Browse);
         CreateSolutionCommand = new RelayCommand(CreateSolution, CanCreateSolution);
         ConfigureExistingCommand = new RelayCommand<SolutionSettings.HistoryItem>(ConfigureExisting);
         ExecuteExistingCommand = new RelayCommand<SolutionSettings.HistoryItem>(RunExisting);
      }

      public Observable<string> Path { get; private set; }
      public ObservableCollection<SolutionSettings.HistoryItem> History { get; private set; }
      public ICommand BrowseCommand { get; private set; }
      public ICommand CreateSolutionCommand { get; private set; }
      public ICommand ConfigureExistingCommand { get; private set; }
      public ICommand ExecuteExistingCommand { get; private set; }

      public void Run()
      {
         _eventBus.Subscribe<ShowLaunchEvent>(_ => _eventBus.Publish(new ShowContentEvent { ViewModel = this }));
         SolutionSettings.Load().Each(x => History.Add(x));
         _eventBus.Publish(new ShowContentEvent { ViewModel = this });
      }

      private void Browse()
      {
         var dialog = new OpenFileDialog
                      {
                         CheckFileExists = true,
                         FileName = Path,
                         // ReSharper disable once LocalizableElement
                         Filter = "Visual Studio solution|*.sln"
                      };
         if (dialog.ShowDialog() != DialogResult.OK) return;
         Path.Value = dialog.FileName;
      }

      private void CreateSolution()
      {
         var item = History.FirstOrDefault(x => x.Path.Equals(Path, StringComparison.CurrentCultureIgnoreCase));
         if (item == null)
         {
            _eventBus.Publish(new OpenNewSolutionEvent
                              {
                                 Path = Path.Value
                              });
         }
         else
         {
            _eventBus.Publish(new OpenSolutionEvent
                              {
                                 Id = item.Id,
                                 ShowSettings = false
                              });
         }
      }

      private bool CanCreateSolution()
      {
         return File.Exists(Path);
      }

      private void ConfigureExisting(SolutionSettings.HistoryItem item)
      {
         _eventBus.Publish(new OpenSolutionEvent
                      {
                         Id = item.Id,
                         ShowSettings = true
                      });
      }

      private void RunExisting(SolutionSettings.HistoryItem item)
      {
         _eventBus.Publish(new OpenSolutionEvent
                      {
                         Id = item.Id,
                         ShowSettings = false
                      });
      }
   }
}
