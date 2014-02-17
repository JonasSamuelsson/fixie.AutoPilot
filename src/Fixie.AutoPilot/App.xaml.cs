using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Fixie.AutoPilot
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

			ShutdownMode = ShutdownMode.OnMainWindowClose;
			MainWindow = new MainWindow();
			MainWindow.Show();
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
