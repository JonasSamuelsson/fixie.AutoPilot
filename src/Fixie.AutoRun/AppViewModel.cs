
namespace Fixie.AutoRun
{
   public class AppViewModel
   {
      private readonly EventBus _bus;

      public AppViewModel()
      {
         _bus = new EventBus();
         _bus.Subscribe<ShowLaunchEvent>(Handle);
         _bus.Subscribe<StartEvent>(Handle);
         ViewModel = new Observable<object>();
      }

	   private void Handle(ShowLaunchEvent @event)
	   {
	      ShowLaunch();
	   }

      private void Handle(StartEvent @event)
      {
         var testViewModel = new ExecuteViewModel(_bus);
         ViewModel.Value = testViewModel;
         testViewModel.Run(@event.SolutionPath);
      }

      public Observable<object> ViewModel { get; private set; }

      public void Run()
      {
	      ShowLaunch();
      }

	   private void ShowLaunch()
	   {
		   var launchViewModel = new LaunchViewModel(_bus);
		   ViewModel.Value = launchViewModel;
		   launchViewModel.Run();
	   }
   }
}