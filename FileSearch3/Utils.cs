﻿using System.IO;
using System.Windows;
using System.Windows.Media;

namespace FileSearch;

public static class Utils
{

	public static bool DirectoryAllowed(string path)
	{
		try
		{
			Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly);
		}
		catch
		{
			return false;
		}
		return true;
	}

	public static void HideMinimizeAndMaximizeButtons(Window window)
	{
		window.SourceInitialized += (s, e) =>
		{
			IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(window).Handle;
			int style = WinApi.GetWindowLong(hwnd, WinApi.GWL_STYLE);

			WinApi.SetWindowLong(hwnd, WinApi.GWL_STYLE, style & ~WinApi.WS_MAXIMIZEBOX & ~WinApi.WS_MINIMIZEBOX);
		};
	}

	public static SolidColorBrush ToBrush(this string colorString)
	{
		try
		{
			return new BrushConverter().ConvertFrom(colorString) as SolidColorBrush;
		}
		catch (Exception)
		{
			return null;
		}
	}

	public static string FixRootPath(string path)
	{
		// Directory.GetDirectories, Directory.GetFiles and Path.Combine does not work on root paths without trailing backslashes.
		if (path.EndsWith(':'))
		{
			return path += "\\";
		}
		return path;
	}

}
