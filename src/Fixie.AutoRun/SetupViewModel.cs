using System;
using System.IO;
using System.Windows.Forms;
using System.Windows.Input;

namespace Fixie.AutoRun
{
    public class SetupViewModel
    {
        private readonly EventBus _eventBus;

        public SetupViewModel(EventBus eventBus)
        {
            _eventBus = eventBus;
            Visible = new Observable<bool>(true);
            Solution = new Observable<string>();
            BrowseCommand = new RelayCommand(Browse);
            GoCommand = new RelayCommand(Go, CanGo);
        }

        private void Browse()
        {
            var dialog = new OpenFileDialog
            {
                CheckFileExists = true,
                FileName = Solution,
                Filter = "Visual Studio solution|*.sln"
            };
            if (dialog.ShowDialog() != DialogResult.OK) return;
            Solution.Value = dialog.FileName;
        }

        private void Go()
        {
            Visible.Value = false;
            _eventBus.Publish(new StartEvent(Solution));
        }

        private bool CanGo()
        {
            return File.Exists(Solution) && Solution.Value.EndsWith(".sln", StringComparison.CurrentCultureIgnoreCase);
        }

        public Observable<bool> Visible { get; private set; }
        public Observable<string> Solution { get; private set; }
        public ICommand BrowseCommand { get; private set; }
        public ICommand GoCommand { get; private set; }
    }
}