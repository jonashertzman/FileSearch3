using System.IO;
using System.Windows;

namespace FileSearch;

internal static class Log
{

	public static Window mainWindow { get; set; }

	public static void LogUnhandledException(Exception exception, string source)
	{
		string errorText = $"{DateTime.UtcNow} - {source}\n{exception.Message}\n{exception.StackTrace}\n\n";

		Directory.CreateDirectory(Path.GetDirectoryName(AppSettings.LogPath));
		File.AppendAllText(AppSettings.LogPath, errorText);

		ExceptionWindow exceptionWindow = new ExceptionWindow()
		{
			Owner = mainWindow,
			ErrorType = source,
			ErrorMessage = exception.Message,
			CallStack = exception.StackTrace
		};

		exceptionWindow.ShowDialog();


		//	MessageBox.Show($"{source}\n{exception.Message}", "Unhandled Exception");
	}

}
