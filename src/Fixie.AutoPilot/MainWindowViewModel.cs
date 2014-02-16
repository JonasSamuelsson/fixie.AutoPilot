using System;
using System.Windows.Forms;
using System.Windows.Input;

namespace Fixie.AutoPilot
{
    public class MainWindowViewModel
    {
        public MainWindowViewModel()
        {
            Setup = new SetupViewModel();
            ShowSetup = new Observable<bool>(true);
        }

        public Observable<bool> ShowSetup { get; private set; }
        public SetupViewModel Setup { get; private set; }
    }

    public class SetupViewModel
    {
        public SetupViewModel()
        {
            Directory = new Observable<string>();
            BrowseCommand = new RelayCommand(Browse);
            GoCommand = new RelayCommand(Go, CanGo);
        }

        private void Browse()
        {
            var dialog = new FolderBrowserDialog
            {
                ShowNewFolderButton = false,
                SelectedPath = Directory
            };
            if (dialog.ShowDialog() != DialogResult.OK) return;
            Directory.Value = dialog.SelectedPath;
        }

        private void Go()
        {
            throw new NotImplementedException();
        }

        private bool CanGo()
        {
            return System.IO.Directory.Exists(Directory);
        }

        public Observable<string> Directory { get; private set; }
        public ICommand BrowseCommand { get; private set; }
        public ICommand GoCommand { get; private set; }
    }
}