using System.IO;
using System.Windows;

namespace FileSearch;

internal static class Log
{

	public static void LogUnhandledException(Exception exception, string source)
	{
		string errorText = $"{DateTime.UtcNow} - {source}\n{exception.Message}\n{exception.StackTrace}\n\n";

		Directory.CreateDirectory(Path.GetDirectoryName(AppSettings.LogPath));
		File.AppendAllText(AppSettings.LogPath, errorText);

		MessageBox.Show($"{source}\n{exception.Message}", "Unhandled Exception");
	}

}
