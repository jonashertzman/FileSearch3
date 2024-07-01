﻿using System.IO;
using System.Windows;

namespace FileSearch;

internal static class Log
{

	public static Window MainWindowInstance { get; set; }

	public static void LogUnhandledException(Exception exception, string source)
	{
		string errorText = $"{DateTime.UtcNow} - {source}\nException:  {exception.GetType().Name}\nMessage:    {exception.Message}\n{exception.StackTrace}\n\n";

		Directory.CreateDirectory(Path.GetDirectoryName(AppSettings.LogPath));
		File.AppendAllText(AppSettings.LogPath, errorText);

		ExceptionWindow exceptionWindow = new()
		{
			Owner = MainWindowInstance,
			ExceptionType = exception.GetType().Name,
			ExceptionMessage = exception.Message,
			Source = source,
			StackTrace = exception.StackTrace
		};

		exceptionWindow.ShowDialog();
	}

}
