using System;
using System.Windows;

namespace Fixie.AutoRun
{
    public class MainWindowViewModel
    {
        public MainWindowViewModel()
        {
            var eventBus = new EventBus();
            Setup = new SetupViewModel(eventBus);
            Content = new ContentViewModel(eventBus);
        }

        public Observable<bool> ShowSetup { get; private set; }
        public SetupViewModel Setup { get; private set; }
        public Observable<bool> ShowContent { get; private set; }
        public ContentViewModel Content { get; private set; }
    }
}