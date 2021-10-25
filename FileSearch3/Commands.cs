using System.Windows.Input;

namespace FileSearch;

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

	public static readonly RoutedUICommand DeleteSearch = new RoutedUICommand("Delete Search", "DeleteSearch", typeof(Commands));

	public static readonly RoutedUICommand DuplicateSearch = new RoutedUICommand("Duplicate Search", "DuplicateSearch", typeof(Commands));

	#endregion

	#region Preview

	public static readonly RoutedUICommand SaveFile = new RoutedUICommand("Save File", "SaveFile", typeof(Commands),
		new InputGestureCollection() { new KeyGesture(Key.S, ModifierKeys.Control) }
	);

	public static readonly RoutedUICommand Edit = new RoutedUICommand("Enable Editing", "Edit", typeof(Commands));

	public static readonly RoutedUICommand NewTab = new RoutedUICommand("New Tab", "NewTab", typeof(Commands),
		new InputGestureCollection() { new KeyGesture(Key.T, ModifierKeys.Control) }
	);

	public static readonly RoutedUICommand PreviousHit = new RoutedUICommand("Move to Previous Hit", "PreviousHit", typeof(Commands),
		new InputGestureCollection() { new KeyGesture(Key.Left) }
	);

	public static readonly RoutedUICommand NextHit = new RoutedUICommand("Move to Next Hit", "NextHit", typeof(Commands),
		new InputGestureCollection() { new KeyGesture(Key.Right) }
	);

	public static readonly RoutedUICommand FirstHit = new RoutedUICommand("Move to First Hit", "FirstHit", typeof(Commands),
		new InputGestureCollection() { new KeyGesture(Key.Left, ModifierKeys.Control) }
	);

	public static readonly RoutedUICommand CurrentHit = new RoutedUICommand("Move to Current Hit", "CurrentHit", typeof(Commands),
		new InputGestureCollection() { new KeyGesture(Key.Space, ModifierKeys.Control) }
	);

	public static readonly RoutedUICommand LastHit = new RoutedUICommand("Move to Last Hit", "LastHit", typeof(Commands),
		new InputGestureCollection() { new KeyGesture(Key.Right, ModifierKeys.Control) }
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

	public static readonly RoutedUICommand CopyResultsToClipboard = new RoutedUICommand("Copy Results to Clipboard", "CopyResultsToClipboard", typeof(Commands));

	public static readonly RoutedUICommand CopyResultsAsCsv = new RoutedUICommand("Copy Results as CSV", "CopyResultsAsCsv", typeof(Commands));

	public static readonly RoutedUICommand RenameTab = new RoutedUICommand("Rename", "RenameTab", typeof(Commands));

	#endregion

}
