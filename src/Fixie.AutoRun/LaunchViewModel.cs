using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;

namespace Fixie.AutoRun
{
   class LaunchViewModel
   {
      private readonly EventBus _bus;

      public LaunchViewModel(EventBus bus)
      {
         _bus = bus;
         Path = new Observable<string>();
         Paths = new ObservableCollection<string>();
         BrowseCommand = new RelayCommand(Browse);
         ExecuteNewCommand = new RelayCommand(ExecuteNew, CanExecuteNew);
         ExecuteExistingCommand = new RelayCommand<string>(ExecuteExisting);
      }

      public Observable<string> Path { get; private set; }
      public ObservableCollection<string> Paths { get; private set; }
      public ICommand BrowseCommand { get; private set; }
      public ICommand ExecuteNewCommand { get; private set; }
      public ICommand ExecuteExistingCommand { get; private set; }

      public void Run()
      {
         LoadHistory().Each(x => Paths.Add(x));
      }

      private static IEnumerable<string> LoadHistory()
      {
         var path = GetHistoryPath();
         return File.Exists(path)
                   ? JsonConvert.DeserializeObject<string[]>(File.ReadAllText(path))
                   : new string[] { };
      }

      private static string GetHistoryPath()
      {
         return System.IO.Path.Combine(Constants.AppDataDirectory, "history.json");
      }

      private void Browse()
      {
         var dialog = new OpenFileDialog
                      {
                         CheckFileExists = true,
                         FileName = Path,
                         Filter = "Visual Studio solution|*.sln"
                      };
         if (dialog.ShowDialog() != DialogResult.OK) return;
         Path.Value = dialog.FileName;
      }

      private void ExecuteNew()
      {
         var solutionPath = Path.Value;

         LoadHistory().Append(solutionPath)
                      .OrderBy(x => x)
                      .Distinct(StringComparer.CurrentCultureIgnoreCase)
                      .Do(SaveHistory);

         _bus.Publish(new StartEvent(solutionPath));
      }

      private static void SaveHistory(IEnumerable<string> history)
      {
         File.WriteAllText(GetHistoryPath(), JsonConvert.SerializeObject(history));
      }

      private bool CanExecuteNew()
      {
         return File.Exists(Path);
      }

      private void ExecuteExisting(string solutionPath)
      {
         _bus.Publish(new StartEvent(solutionPath));
      }
   }
}
