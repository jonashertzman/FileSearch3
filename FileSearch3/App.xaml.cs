using System.Windows;

namespace FileSearch;

public partial class App : Application
{

	public App()
	{
		AppDomain.CurrentDomain.UnhandledException += (s, e) =>
		{
			Log.LogUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException", this.MainWindow);
		};

		DispatcherUnhandledException += (s, e) =>
		{
			Log.LogUnhandledException(e.Exception, "Application.Current.DispatcherUnhandledException", this.MainWindow);
			e.Handled = true;
		};

		TaskScheduler.UnobservedTaskException += (s, e) =>
		{
			Log.LogUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException", this.MainWindow);
			e.SetObserved();
		};
	}

}