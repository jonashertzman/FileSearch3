using System.IO;

namespace FileSearch;

internal static class Log
{

	public static void LogUnhandledException(Exception exception, string source, System.Windows.Window mainWindow)
	{
		string errorText = $"{DateTime.UtcNow} - {source}\n{exception.Message}\n{exception.StackTrace}\n\n";

		Directory.CreateDirectory(Path.GetDirectoryName(AppSettings.LogPath));
		File.AppendAllText(AppSettings.LogPath, errorText);

		ExceptionWindow exceptionWindow = new ExceptionWindow() { Owner = mainWindow };

		exceptionWindow.ShowDialog();


		//	MessageBox.Show($"{source}\n{exception.Message}", "Unhandled Exception");
	}

}
