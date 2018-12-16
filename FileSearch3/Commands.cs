using System.Windows.Input;

namespace FileSearch
{
	public static class Commands
	{

		#region Main Menu

		public static readonly RoutedUICommand Exit = new RoutedUICommand("Exit", "Exit", typeof(Commands),
			new InputGestureCollection() { new KeyGesture(Key.F4, ModifierKeys.Alt) }
		);

		public static readonly RoutedUICommand Options = new RoutedUICommand("Options", "Options", typeof(Commands));

		public static readonly RoutedUICommand About = new RoutedUICommand("About", "About", typeof(Commands));

		#endregion

		#region Main Toolbar

		public static readonly RoutedUICommand StartSearch = new RoutedUICommand("Start Search", "StartSearch", typeof(Commands),
			new InputGestureCollection() { new KeyGesture(Key.F5) }
		);

		public static readonly RoutedUICommand StopSearch = new RoutedUICommand("Stop Search", "StopSearch", typeof(Commands),
			new InputGestureCollection() { new KeyGesture(Key.F5, ModifierKeys.Shift) }
		);

		#endregion

		#region Preview

		public static readonly RoutedUICommand SaveFile = new RoutedUICommand("Save File", "SaveFile", typeof(Commands));

		public static readonly RoutedUICommand Edit = new RoutedUICommand("Enable Editing", "Edit", typeof(Commands));


		public static readonly RoutedUICommand PreviousDiff = new RoutedUICommand("Move to Previous Diff", "PreviousDiff", typeof(Commands),
			new InputGestureCollection() { new KeyGesture(Key.Up) }
		);

		public static readonly RoutedUICommand NextDiff = new RoutedUICommand("Move to Next Diff", "NextDiff", typeof(Commands),
			new InputGestureCollection() { new KeyGesture(Key.Down) }
		);

		public static readonly RoutedUICommand FirstDiff = new RoutedUICommand("Move to First Diff", "FirstDiff", typeof(Commands),
			new InputGestureCollection() { new KeyGesture(Key.Up, ModifierKeys.Control) }
		);

		public static readonly RoutedUICommand CurrentDiff = new RoutedUICommand("Move to Current Diff", "CurrentDiff", typeof(Commands),
			new InputGestureCollection() { new KeyGesture(Key.Space, ModifierKeys.Control) }
		);

		public static readonly RoutedUICommand LastDiff = new RoutedUICommand("Move to Last Diff", "LastDiff", typeof(Commands),
			new InputGestureCollection() { new KeyGesture(Key.Down, ModifierKeys.Control) }
		);

		public static readonly RoutedUICommand Find = new RoutedUICommand("Find", "Find", typeof(Commands),
			new InputGestureCollection() { new KeyGesture(Key.F, ModifierKeys.Control) }
		);

		public static readonly RoutedUICommand FindNext = new RoutedUICommand("Find Next", "FindNext", typeof(Commands),
			new InputGestureCollection() { new KeyGesture(Key.G, ModifierKeys.Control), new KeyGesture(Key.F3) }
		);

		public static readonly RoutedUICommand FindPrevious = new RoutedUICommand("Find Previous", "FindPrevious", typeof(Commands),
			new InputGestureCollection() { new KeyGesture(Key.G, ModifierKeys.Control | ModifierKeys.Shift) }
		);

		public static readonly RoutedUICommand CloseFind = new RoutedUICommand("Close Find", "CloseFind", typeof(Commands),
			new InputGestureCollection() { new KeyGesture(Key.Escape) }
		);

		public static readonly RoutedUICommand OpenContainingFolder = new RoutedUICommand("Open Containing Folder", "OpenContainingFolder", typeof(Commands));

		public static readonly RoutedUICommand CopyPathToClipboard = new RoutedUICommand("Copy Path to Clipboard", "CopyPathToClipboard", typeof(Commands));

		#endregion

	}
}
