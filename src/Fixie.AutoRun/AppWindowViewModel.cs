
using Fixie.AutoRun.Events;
using Fixie.AutoRun.Infrastructure;

namespace Fixie.AutoRun
{
   public class AppWindowViewModel
   {
      private readonly EventBus _bus;

      public AppWindowViewModel()
      {
         _bus = new EventBus();
         _bus.Subscribe<ShowLaunchEvent>(Handle);
         _bus.Subscribe<StartEvent>(Handle);
         _bus.Subscribe<ShowFlyoutEvent>(Handle);
         MainViewModel = new Observable<object>();
         ShowFlyout = new Observable<bool>();
      }

      private void Handle(ShowFlyoutEvent obj)
      {
         ShowFlyout.Value = true;
      }

      private void Handle(ShowLaunchEvent @event)
      {
         ShowLaunch();
      }

      private void Handle(StartEvent @event)
      {
         var executeViewModel = new ExecuteViewModel(_bus);
         MainViewModel.Value = executeViewModel;
         executeViewModel.Run(@event.SolutionPath);
      }

      public Observable<object> MainViewModel { get; private set; }
      public Observable<bool> ShowFlyout { get; private set; }

      public void Run()
      {
         ShowLaunch();
      }

      private void ShowLaunch()
      {
         var launchViewModel = new LaunchViewModel(_bus);
         MainViewModel.Value = launchViewModel;
         launchViewModel.Run();
      }
   }
}