using Fixie.AutoRun.Events;
using Fixie.AutoRun.Infrastructure;
using System.Linq;

namespace Fixie.AutoRun
{
   public class AppWindowViewModel
   {
      private readonly EventBus _eventBus;
      private readonly IViewModel[] _viewModels;

      public AppWindowViewModel()
      {
         _eventBus = new EventBus();
         _viewModels = new IViewModel[]
                              {
                                 new LaunchViewModel(_eventBus),
                                 new ExecuteViewModel(_eventBus),
                                 new SettingsViewModel(_eventBus)
                              };
         ContentViewModel = new Observable<IContentViewModel>();
      }

      public Observable<IContentViewModel> ContentViewModel { get; private set; }

      public SettingsViewModel SettingsViewModel
      {
         get { return _viewModels.OfType<SettingsViewModel>().Single(); }
      }

      public void Run()
      {
         _eventBus.Subscribe<ShowContentEvent>(ShowContent);
         _viewModels.Each(x => x.Run());
      }

      private void ShowContent(ShowContentEvent @event)
      {
         ContentViewModel.Value = @event.ViewModel;
      }
   }
}