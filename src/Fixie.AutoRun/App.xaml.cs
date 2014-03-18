using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Fixie.AutoRun
{
   /// <summary>
   /// Interaction logic for App.xaml
   /// </summary>
   public partial class App : Application
   {
      private void App_OnStartup(object sender, StartupEventArgs e)
      {
         AppDomain.CurrentDomain.UnhandledException += (s, eventArgs) => Error((Exception)eventArgs.ExceptionObject);
         Dispatcher.UnhandledException += OnDispatcherUnhandledException;
         Current.DispatcherUnhandledException += OnDispatcherUnhandledException;
         TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;

         AssertAppDataDirectory();

         ShutdownMode = ShutdownMode.OnLastWindowClose;
         var window = new AppWindow();
         window.Show();
         ((AppViewModel)window.DataContext).Run();
      }

      private void AssertAppDataDirectory()
      {
         var directory = Constants.AppDataDirectory;
         if (!Directory.Exists(directory))
         {
            Directory.CreateDirectory(directory);
            while (!Directory.Exists(directory)) Thread.Sleep(0);
         }
      }

      private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs eventArgs)
      {
         Error(eventArgs.Exception);
         eventArgs.Handled = true;
      }

      private static void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs eventArgs)
      {
         Error(eventArgs.Exception);
         eventArgs.SetObserved();
      }

      public static void Error(Exception exception)
      {
         MessageBox.Show(exception.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
      }
   }
}
