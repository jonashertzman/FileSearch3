using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;

namespace FileSearch;

public partial class MainWindow : Window
{

	#region Members

	MainWindowViewModel ViewModel { get; set; } = new MainWindowViewModel();

	readonly DispatcherTimer updatePreviewTimer = new();

	int firstHit = -1;
	int lastHit = -1;

	readonly int standardColumnCount = 0;
	string currentColumnSetup = "";

	#endregion

	#region Constructor

	public MainWindow()
	{
		InitializeComponent();

		DataContext = ViewModel;

		SearchPanel.Visibility = Visibility.Collapsed;

		updatePreviewTimer.Interval = new TimeSpan(10000);
		updatePreviewTimer.Tick += UpdatePreviewTimer_Tick;

		standardColumnCount = dataGridFileList.Columns.Count;

		UpdateStats();
		Log.MainWindowInstance = this;
	}

	#endregion

	#region Properties

	public SearchInstance ActiveSearch
	{
		get
		{
			return ViewModel.ActiveSearchInstance;
		}
	}

	#endregion

	#region Methods

	private void LoadSettings()
	{
		AppSettings.LoadSettings();

		this.Left = AppSettings.PositionLeft;
		this.Top = AppSettings.PositionTop;
		this.Width = AppSettings.Width;
		this.Height = AppSettings.Height;
		this.WindowState = AppSettings.WindowState;

		if (ViewModel.SearchInstances.Count == 0)
		{
			ViewModel.SearchInstances.Add(new SearchInstance() { Selected = true });
		}

		foreach (SearchInstance s in AppSettings.SearchInstances)
		{
			if (s.Selected)
			{
				ViewModel.ActiveSearchInstance = s;
				break;
			}
		}

		// Only for backwards compatibility against old settings file.
		if (ViewModel.ActiveSearchInstance == null)
		{
			ViewModel.SearchInstances[0].Selected = true;
			ViewModel.ActiveSearchInstance = ViewModel.SearchInstances[0];
		}
	}

	private void SaveSettings()
	{
		CleanSearchAttributes();

		AppSettings.PositionLeft = this.Left;
		AppSettings.PositionTop = this.Top;
		AppSettings.Width = this.Width;
		AppSettings.Height = this.Height;
		AppSettings.WindowState = this.WindowState;

		AppSettings.WriteSettingsToDisk();
	}

	private void CleanSearchAttributes()
	{
		foreach (SearchInstance s in AppSettings.SearchInstances)
		{
			s.SearchPhrases = new ObservableCollection<TextAttribute>(s.SearchPhrases.DistinctBy(x => x.Text));
			for (int i = s.SearchPhrases.Count - 1; i >= 0; i--)
			{
				if (string.IsNullOrEmpty(s.SearchPhrases[i].Text))
				{
					s.SearchPhrases.RemoveAt(i);
				}
			}

			s.SearchDirectories = new ObservableCollection<TextAttribute>(s.SearchDirectories.DistinctBy(x => x.UppercaseText));
			for (int i = s.SearchDirectories.Count - 1; i >= 0; i--)
			{
				if (string.IsNullOrEmpty(s.SearchDirectories[i].Text))
				{
					s.SearchDirectories.RemoveAt(i);
				}
			}

			s.SearchFiles = new ObservableCollection<TextAttribute>(s.SearchFiles.DistinctBy(x => x.UppercaseText));
			for (int i = s.SearchFiles.Count - 1; i >= 0; i--)
			{
				if (string.IsNullOrEmpty(s.SearchFiles[i].Text))
				{
					s.SearchFiles.RemoveAt(i);
				}
			}
		}
	}

	private void AddNewSearch(string searchDirectory = "")
	{
		SearchInstance newInstance = new();
		if (searchDirectory != "")
		{
			newInstance.SearchDirectories.Add(new TextAttribute(searchDirectory));
		}
		ViewModel.SearchInstances.Add(newInstance);
		SetActiveTab(newInstance);
	}

	private void SetActiveTab(SearchInstance searchInstance)
	{
		ViewModel.ActiveSearchInstance = searchInstance;

		foreach (SearchInstance s in AppSettings.SearchInstances)
		{
			s.Selected = s == searchInstance;
		}

		UpdateStats();
	}

	internal void UpdateStats()
	{
		string newColumnSetup = ActiveSearch.PhraseColumnSetup;

		if (newColumnSetup != currentColumnSetup)
		{
			while (dataGridFileList.Columns.Count > standardColumnCount)
			{
				dataGridFileList.Columns.RemoveAt(standardColumnCount);
			}

			int i = 0;
			foreach (string s in ActiveSearch.StoredSearchPhrases)
			{
				dataGridFileList.Columns.Add(new DataGridTextColumn()
				{
					Header = $"{s}",
					Binding = ActiveSearch.CaseSensitive ? new Binding($"PhraseHitsList[{i}].CaseSensitiveCount") : new Binding($"PhraseHitsList[{i}].Count"),
					CellStyle = (Style)FindResource("RightAlignedCell")
				});
				i++;
			}
			currentColumnSetup = newColumnSetup;
		}

		foreach (string s in ActiveSearch.StoredSearchPhrases)
		{
			ActiveSearch.PhraseSums[s] = 0;
		}

		int filesFound = 0;

		foreach (FileHit f in ActiveSearch.FilesWithHits)
		{
			if (ActiveSearch.FindAllPhrases ? f.AllPhrasesHit(ActiveSearch.CaseSensitive) : f.AnyPhraseHit(ActiveSearch.CaseSensitive))
			{
				filesFound++;
				f.Visible = true;

				foreach (string s in ActiveSearch.StoredSearchPhrases)
				{
					ActiveSearch.PhraseSums[s] += f.PhraseHits[s].GetCount(ActiveSearch.CaseSensitive);
				}
			}
			else
			{
				f.Visible = false;
			}
		}

		int columnIndex = standardColumnCount;
		foreach (string s in ActiveSearch.StoredSearchPhrases)
		{
			dataGridFileList.Columns[columnIndex].Header = $"{s} ({ActiveSearch.PhraseSums[s]})";
			columnIndex++;
		}

		string temp = $"{filesFound} files found";
		if (ActiveSearch.SearchedFileCount > 0)
		{
			temp += $" in {ActiveSearch.SearchedFileCount} searched";
		}

		ActiveSearch.FileCountStatus = temp;
		ActiveSearch.ErrorCountStatus = $"{ActiveSearch.Errors.Count} Errors";
		ActiveSearch.IgnoredFilesCountStatus = $"{ActiveSearch.IgnoredFileCount} Ignored";
	}

	private void UpdatePreview()
	{
		Debug.Print("UpdatePreview");

		Mouse.OverrideCursor = Cursors.Wait;

		ObservableCollection<Line> Lines = [];
		List<FileHit> previewFiles = [];

		firstHit = -1;
		lastHit = -1;

		foreach (FileHit f in dataGridFileList.Items)
		{
			if (f.Selected && f.Visible)
			{
				previewFiles.Add(f);
			}
		}

		foreach (FileHit currentFile in previewFiles)
		{
			string[] allLines = [];
			string allText = "";
			int lineNumber = 1;

			if (Directory.Exists(currentFile.Path))
			{
				if (previewFiles.Count > 1)
				{
					Lines.Add(new Line() { Type = TextState.Header, Text = currentFile.Path });
				}
				Lines.Add(new Line() { Type = TextState.SurroundSpacing, Text = "[FOLDER]" });
			}
			else
			{
				try
				{
					ViewModel.FileEncoding = Unicode.GetEncoding(currentFile.Path);
					ViewModel.FileEdited = false;

					if (previewFiles.Count > 1)
					{
						Lines.Add(new Line() { Type = TextState.Header, Text = currentFile.Path });
					}

					if (ActiveSearch.RegexSearch)
					{
						allText = File.ReadAllText(currentFile.Path, ViewModel.FileEncoding.Type);
						if (!(allText.EndsWith("\r\n") || allText.EndsWith('\r') || allText.EndsWith('\n')))
						{
							allText += ViewModel.FileEncoding.GetNewLineString;
						}
					}
					else
					{
						allLines = File.ReadAllLines(currentFile.Path, ViewModel.FileEncoding.Type);
					}

				}
				catch (Exception e)
				{
					MessageBox.Show(e.Message, e.GetType().Name);
					continue;
				}

				if (ActiveSearch.RegexSearch)
				{
					if (ValidateHitsRegex())
					{
						List<RegexHit> regexHits = [];
						foreach (string searchPhrase in ActiveSearch.StoredSearchPhrases)
						{
							Match match = Regex.Match(allText, searchPhrase, ActiveSearch.CaseSensitive ? RegexOptions.Multiline : RegexOptions.Multiline | RegexOptions.IgnoreCase);
							while (match.Success && match.Length > 0)
							{
								regexHits.Add(new RegexHit(match.Index, match.Length));
								match = match.NextMatch();
							}
						}

						int lineSourceIndex = 0; // Start index for the current line including all new line characters in the source file.

						MatchCollection newLines = Regex.Matches(allText, "(\r\n|\r|\n)");

						foreach (Match newLine in newLines)
						{
							Line previewLine = new()
							{
								Text = allText[lineSourceIndex..newLine.Index],
								CurrentFile = currentFile.Path,
								LineNumber = lineNumber++
							};

							int lineSourceLength = previewLine.Text.Length + newLine.Length; // Length of current line including all new line characters.

							bool[] hitCharacters = new bool[previewLine.Text.Length];

							foreach (RegexHit regexHit in regexHits)
							{
								if (regexHit.Start < lineSourceIndex + lineSourceLength && lineSourceIndex <= regexHit.Start + regexHit.Length)
								{
									previewLine.Type = TextState.Hit;
									int selectionStartIndex = Math.Max(regexHit.Start - lineSourceIndex, 0);
									int selectionEndIndex = Math.Min(regexHit.Start - lineSourceIndex + regexHit.Length, previewLine.Text.Length);

									for (int i = selectionStartIndex; i < selectionEndIndex; i++)
									{
										hitCharacters[i] = true;
									}
								}
							}

							if (previewLine.Type == TextState.Hit)
							{
								previewLine.AddHitSegments(hitCharacters);
							}

							Lines.Add(previewLine);

							lineSourceIndex += lineSourceLength;
						}
					}
				}
				else
				{
					foreach (string line in allLines)
					{
						Line previewLine = new();
						bool[] hitCharacters = new bool[line.Length];
						previewLine.Text = line;
						previewLine.CurrentFile = currentFile.Path;
						previewLine.LineNumber = lineNumber++;

						foreach (string phrase in ActiveSearch.StoredSearchPhrases)
						{
							int hitIndex = 0;
							while (true)
							{
								hitIndex = line.IndexOf(phrase, hitIndex, ActiveSearch.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
								if (hitIndex == -1)
								{
									break;
								}

								for (int i = hitIndex; i < hitIndex + phrase.Length; i++)
								{
									hitCharacters[i] = true;
								}

								hitIndex += phrase.Length;
								previewLine.Type = TextState.Hit;
							}
						}

						if (previewLine.Type == TextState.Hit)
						{
							previewLine.AddHitSegments(hitCharacters);
						}

						Lines.Add(previewLine);
					}
				}
			}
			if (previewFiles.Count > 1)
			{
				Lines.Add(new Line() { Type = TextState.Filler, Text = "" });
			}
		}

		CurrentFilePanel.Visibility = previewFiles.Count > 1 ? Visibility.Visible : Visibility.Collapsed;

		if (ActiveSearch.ShowOnlyHits)
		{
			RemoveMissLines(ref Lines);
		}

		int maxLineNumber = 1;
		for (int i = 0; i < Lines.Count; i++)
		{
			if (Lines[i].LineNumber > maxLineNumber)
				maxLineNumber = (int)Lines[i].LineNumber;

			if (Lines[i].Type == TextState.Hit)
			{
				lastHit = i;
				if (firstHit == -1)
				{
					firstHit = lastHit;
				}
			}
		}

		ViewModel.EditMode = false;
		ViewModel.PreviewLines = Lines;

		Preview.Init(maxLineNumber.ToString().Length);

		MoveToFirstHit();

		Mouse.OverrideCursor = null;
	}

	private void RemoveMissLines(ref ObservableCollection<Line> Lines)
	{

		if (ActiveSearch.SurroundingLines > 0)
		{
			for (int i = 0; i < Lines.Count; i++)
			{
				if (Lines[i].Type == TextState.Hit)
				{
					for (int j = 1; j <= ActiveSearch.SurroundingLines; j++)
					{
						if (i - j >= 0 && Lines[i - j].Type == TextState.Miss)
						{
							Lines[i - j].Type = TextState.Surround;
						}
						else
						{
							break;
						}
					}

					for (int j = 1; j <= ActiveSearch.SurroundingLines; j++)
					{
						if (i + j < Lines.Count && Lines[i + j].Type == TextState.Miss)
						{
							Lines[i + j].Type = TextState.Surround;
						}
						else
						{
							break;
						}
					}
				}
			}
		}

		bool spaceInserted = false;

		for (int i = Lines.Count - 1; i >= 0; i--)
		{
			if (Lines[i].Type == TextState.Miss)
			{
				if (!spaceInserted && ActiveSearch.SurroundingLines > 0 && ActiveSearch.StoredSearchPhrases.Count > 0)
				{
					Lines[i].Type = TextState.SurroundSpacing;
					Lines[i].Text = "- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -";
					Lines[i].LineNumber = null;
					spaceInserted = true;
				}
			}
			else
			{
				spaceInserted = false;
			}
		}

		// Removing items from a collection is slow, much faster to create a new collection and add all wanted items. 
		ObservableCollection<Line> newLines = [];
		foreach (Line l in Lines)
		{
			if (l.Type != TextState.Miss)
			{
				newLines.Add(l);
			}
		}

		Lines = newLines;
	}

	private void MoveToFirstHit()
	{
		ViewModel.CurrentHit = -1;
		MoveToNextHit();
	}

	private void MoveToLastHit()
	{
		ViewModel.CurrentHit = ViewModel.PreviewLines.Count;
		MoveToPreviousHit();
	}

	private void MoveToPreviousHit()
	{
		for (int i = ViewModel.CurrentHit - 1; i >= 0; i--)
		{
			if (ViewModel.PreviewLines[i].Type == TextState.Hit)
			{
				CurrentFile.Text = ViewModel.PreviewLines[i].CurrentFile;
				ViewModel.CurrentHit = i;
				CenterOnLine(i);
				return;
			}
		}
	}

	private void MoveToNextHit()
	{
		for (int i = ViewModel.CurrentHit + 1; i < ViewModel.PreviewLines.Count; i++)
		{
			if (ViewModel.PreviewLines[i].Type == TextState.Hit)
			{
				CurrentFile.Text = ViewModel.PreviewLines[i].CurrentFile;
				ViewModel.CurrentHit = i;
				CenterOnLine(i);
				return;
			}
		}
	}

	private void CenterOnLine(int i)
	{
		int visibleLines = Preview.VisibleLines <= 0 ? (int)(Preview.ActualHeight / OneCharacter.ActualHeight) : Preview.VisibleLines;
		VerticalScrollbar.Value = i - (visibleLines / 2) + 1;
	}

	private void ProcessSearchResult(int result)
	{
		if (result != -1)
		{
			SearchBox.Error = false;
			CenterOnLine(result);
		}
		else
		{
			SearchBox.Error = true;
		}
	}

	private bool ValidateRegexPhrases()
	{
		if (ActiveSearch.RegexSearch)
		{
			foreach (TextAttribute p in ActiveSearch.SearchPhrases)
			{
				if (p.Used)
				{
					try
					{
						Regex.Match("", p.Text);
					}
					catch (ArgumentException)
					{
						MessageBox.Show($"{p.Text} is not a valid regular expression", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
						return false;
					}
				}
			}
		}
		return true;
	}

	private bool ValidateHitsRegex()
	{
		if (ActiveSearch.RegexSearch)
		{
			foreach (string s in ActiveSearch.StoredSearchPhrases)
			{
				try
				{
					Regex.Match("", s);
				}
				catch (ArgumentException)
				{
					MessageBox.Show($"{s} is not a valid regular expression", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					return false;
				}
			}
		}
		return true;
	}

	private async void CheckForNewVersion(bool forced = false)
	{
		if (AppSettings.CheckForUpdates && AppSettings.LastUpdateTime < DateTime.Now.AddDays(-5) || forced)
		{
			try
			{
				Debug.Print("Checking for new version...");

				HttpClient httpClient = new();
				string result = await httpClient.GetStringAsync("https://jonashertzman.github.io/FileSearch3/download/version.txt");

				Debug.Print($"Latest version found: {result}");
				ViewModel.NewBuildAvailable = int.Parse(result) > int.Parse(ViewModel.BuildNumber);
			}
			catch (Exception exception)
			{
				Debug.Print($"Version check failed: {exception.Message}");
			}

			AppSettings.LastUpdateTime = DateTime.Now;
		}
	}

	#endregion

	#region Events

	private void Window_Closed(object sender, EventArgs e)
	{
		SaveSettings();
	}

	private void Window_Initialized(object sender, EventArgs e)
	{
		LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

		LoadSettings();
		CheckForNewVersion();
	}

	private void Window_ContentRendered(object sender, EventArgs e)
	{
		if (Environment.GetCommandLineArgs().Length > 1)
		{
			AddNewSearch(Environment.GetCommandLineArgs()[1]);
		}
	}

	private void TabButton_Click(object sender, RoutedEventArgs e)
	{
		SetActiveTab((SearchInstance)((Button)sender).Tag);
	}

	private void TabButton_ContextMenuOpening(object sender, ContextMenuEventArgs e)
	{
		SetActiveTab((SearchInstance)((Button)sender).Tag);
	}

	private void DataGridFileList_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		//Debug.Print($"Added {e.AddedItems.Count}");
		//Debug.Print($"Removed {e.RemovedItems.Count}");
		//Debug.Print($"SelectedItems {dataGridFileList.SelectedItems.Count}");
		//Debug.Print($"Items {dataGridFileList.Items.Count}");

		foreach (FileHit f in e.AddedItems)
		{
			f.Selected = true;
		}
		foreach (FileHit f in e.RemovedItems)
		{
			f.Selected = false;
		}

		updatePreviewTimer.Start();
	}

	private void ToggleButton_Checked(object sender, RoutedEventArgs e)
	{
		UpdateStats();
		UpdatePreview();
	}

	private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		UpdatePreview();
	}

	private void UpdatePreviewTimer_Tick(object sender, EventArgs e)
	{
		updatePreviewTimer.Stop();
		UpdatePreview();
	}

	private void DataGridFileList_Sorting(object sender, DataGridSortingEventArgs e)
	{
		updatePreviewTimer.Start();
	}

	private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
	{
		ProcessSearchResult(Preview.Search(SearchBox.Text, MatchCase.IsChecked == true));
	}

	private void MatchCase_Checked(object sender, RoutedEventArgs e)
	{
		ProcessSearchResult(Preview.Search(SearchBox.Text, MatchCase.IsChecked == true));
	}

	private void Preview_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
	{
		bool controlPressed = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;

		if (controlPressed)
		{
			ViewModel.Zoom += e.Delta / Math.Abs(e.Delta);
		}
		else
		{
			int lines = SystemParameters.WheelScrollLines * e.Delta / 120;
			VerticalScrollbar.Value -= lines;
		}
	}

	private void DataGridFileList_RowDoubleClick(object sender, MouseButtonEventArgs e)
	{
		using Process p = new();

		p.StartInfo.FileName = ((FileHit)((DataGridRow)sender).Item).Path;
		p.StartInfo.ErrorDialog = true;
		p.StartInfo.UseShellExecute = true;
		p.Start();
	}

	private void DataGridCell_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		e.Handled = true;
	}

	private void DataGridCell_PreviewMouseDown(object sender, MouseButtonEventArgs e)
	{
		e.Handled = true;
	}

	private void DataGridCell_MouseUp(object sender, MouseButtonEventArgs e)
	{
		FileHit x = ((DataGridCell)sender).DataContext as FileHit;
		if (e.ChangedButton == MouseButton.Right)
		{
			x.Flag = 0;
		}
		else
		{
			x.Flag++;
		}
		e.Handled = true;
	}

	private void BrowseDirectoryButton_Click(object sender, RoutedEventArgs e)
	{
		TextAttribute t = (sender as Button).DataContext as TextAttribute;

		BrowseFolderWindow browseFolderWindow = new() { DataContext = ViewModel, Owner = this, SelectedPath = t?.Text };
		browseFolderWindow.ShowDialog();

		if (browseFolderWindow.DialogResult == true)
		{
			if (t == null)
			{
				ActiveSearch.SearchDirectories.Add(new TextAttribute(browseFolderWindow.SelectedPath));
			}
			else
			{
				t.Text = browseFolderWindow.SelectedPath;
			}
		}
	}

	private void ErrorCountHyperlink_Click(object sender, RoutedEventArgs e)
	{
		LogWindow logWindow = new() { DataContext = ActiveSearch, Owner = this, Type = LogWindowType.Errors };
		logWindow.ShowDialog();
	}

	private void IgnoredFilesCountHyperlink_Click(object sender, RoutedEventArgs e)
	{
		LogWindow logWindow = new() { DataContext = ActiveSearch, Owner = this, Type = LogWindowType.IgnoredFiles };
		logWindow.ShowDialog();
	}

	private void Hyperlink_OpenHomepage(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
	{
		Process.Start(new ProcessStartInfo(e.Uri.ToString()) { UseShellExecute = true });
		e.Handled = true;
	}

	private void DataGridSearchPhrases_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
		{
			if (Clipboard.ContainsText())
			{
				foreach (string line in ((string)Clipboard.GetText()).Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None))
				{
					ViewModel.ActiveSearchInstance.SearchPhrases.Add(new TextAttribute(line));
				}
			}
			e.Handled = true;
		}
		e.Handled = false;
	}

	private void LightMode_Click(object sender, RoutedEventArgs e)
	{
		ViewModel.Theme = Themes.Light;
	}

	private void DarkMode_Click(object sender, RoutedEventArgs e)
	{
		ViewModel.Theme = Themes.Dark;
	}

	#endregion

	#region Commands

	#region Menues

	private void CommandExit_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
	{
		this.Close();
	}

	private void CommandNewTab_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		AddNewSearch();
	}

	private void CommandOptions_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
	{
		// Store existing settings data in case the changes are canceled.
		var oldCheckForUpdates = ViewModel.CheckForUpdates;
		var oldFont = ViewModel.Font;
		var oldFontSize = ViewModel.FontSize;
		var oldTabSize = ViewModel.TabSize;
		var oldIgnoredFiles = new ObservableCollection<TextAttribute>(ViewModel.IgnoredFiles);
		var oldIgnoredDirectories = new ObservableCollection<TextAttribute>(ViewModel.IgnoredDirectories);
		var oldDarkTheme = AppSettings.DarkTheme.Clone();
		var oldLightTheme = AppSettings.LightTheme.Clone();
		var oldTheme = ViewModel.Theme;

		OptionsWindow optionsWindow = new() { DataContext = ViewModel, Owner = this };
		optionsWindow.ShowDialog();

		if (optionsWindow.DialogResult == true)
		{
			SaveSettings();
		}
		else
		{
			// Options window was canceled, revert to old settings.
			ViewModel.CheckForUpdates = oldCheckForUpdates;
			ViewModel.Font = oldFont;
			ViewModel.FontSize = oldFontSize;
			ViewModel.TabSize = oldTabSize;
			ViewModel.IgnoredFiles = new ObservableCollection<TextAttribute>(oldIgnoredFiles);
			ViewModel.IgnoredDirectories = new ObservableCollection<TextAttribute>(oldIgnoredDirectories);
			AppSettings.DarkTheme = oldDarkTheme;
			AppSettings.LightTheme = oldLightTheme;
			ViewModel.Theme = oldTheme;
		}
	}

	private void CommandAbout_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
	{
		CheckForNewVersion(true);

		AboutWindow aboutWindow = new() { Owner = this, DataContext = ViewModel };
		aboutWindow.ShowDialog();
	}

	private void CommandRenameTab_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		RenameTabWindow renameTabWindow = new() { TabName = ActiveSearch.Name };
		renameTabWindow.ShowDialog();

		if (renameTabWindow.DialogResult == true)
		{
			ActiveSearch.Name = renameTabWindow.TabName;
			ActiveSearch.Renamed = true;
		}
	}

	private void CommandOpenContainingFolder_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		string args = $"/Select, {((FileHit)dataGridFileList.SelectedItem).Path}";
		ProcessStartInfo pfi = new("Explorer.exe", args);
		Process.Start(pfi);
	}

	private void CommandOpenContainingFolder_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = dataGridFileList.SelectedItems.Count == 1;
	}

	private void CommandCopyPathToClipboard_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		WinApi.CopyTextToClipboard(Path.GetFullPath(((FileHit)dataGridFileList.SelectedItem).Path));
	}

	private void CommandCopyPathToClipboard_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = dataGridFileList.SelectedItems.Count == 1;
	}

	private void CommandCopyResultsToClipboard_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		StringBuilder s = new();

		foreach (FileHit r in dataGridFileList.Items)
		{
			if (r.Visible)
			{
				s.Append(r.Path + "\r\n");
			}
		}

		if (s.ToString() != "")
		{
			WinApi.CopyTextToClipboard(s.ToString());
		}
	}

	private void CommandCopyResultsToClipboard_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = dataGridFileList.Items.Count > 0;
	}

	private void CommandCopyResultsAsCsv_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		StringBuilder s = new();

		s.Append("File,Size,Date");
		foreach (string phrase in ActiveSearch.StoredSearchPhrases)
		{
			s.Append($",\"{phrase.Replace("\"", "\"\"")}\"");
		}
		s.Append("\r\n");

		foreach (FileHit r in dataGridFileList.Items)
		{
			if (r.Visible)
			{
				s.Append($"\"{r.Path}\",\"{r.Size}\",\"{r.Date}\"");
				foreach (KeyValuePair<string, PhraseHit> kvp in r.PhraseHits)
				{
					s.Append($",{kvp.Value.Count}");
				}

				s.Append("\r\n");
			}
		}

		if (s.ToString() != "")
		{
			WinApi.CopyTextToClipboard(s.ToString());
		}
	}

	private void CommandCopyResultsAsCsv_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = dataGridFileList.Items.Count > 0;
	}

	private void CommandZoomIn_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		ViewModel.Zoom += 1;
	}

	private void CommandZoomOut_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		ViewModel.Zoom -= 1;
	}

	private void CommandResetZoom_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		ViewModel.Zoom = 0;
	}

	#endregion

	#region Main Toolbar

	private void CommandStartSearch_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
	{
		if (!ValidateRegexPhrases())
		{
			return;
		}

		DataGridSearchPhrases.CommitEdit();
		DataGridSearchDirectories.CommitEdit();
		DataGridSearcFiles.CommitEdit();

		CleanSearchAttributes();

		ActiveSearch.PhraseSums.Clear();

		foreach (TextAttribute t in ActiveSearch.SearchPhrases)
		{
			ActiveSearch.PhraseSums.Add(t.Text, 0);
		}

		ActiveSearch.StartSearch(this);
	}

	private void CommandStopSearch_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
	{
		ActiveSearch.CancelSearch();
	}

	private void CommandDeleteSearch_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
	{
		int removedIndex = ViewModel.SearchInstances.IndexOf(ActiveSearch);
		ViewModel.SearchInstances.RemoveAt(removedIndex);
		if (ViewModel.SearchInstances.Count > 0)
		{
			SetActiveTab(ViewModel.SearchInstances[Math.Max(0, removedIndex - 1)]);
		}
		else
		{
			AddNewSearch();
		}
	}

	private void CommandDuplicateSearch_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
	{
		SearchInstance newInstance = new() { CaseSensitive = ActiveSearch.CaseSensitive, RegexSearch = ActiveSearch.RegexSearch };

		foreach (TextAttribute attribute in ActiveSearch.SearchPhrases)
		{
			newInstance.SearchPhrases.Add(new TextAttribute(attribute.Text, attribute.Used));
		}

		foreach (TextAttribute attribute in ActiveSearch.SearchDirectories)
		{
			newInstance.SearchDirectories.Add(new TextAttribute(attribute.Text, attribute.Used));
		}

		foreach (TextAttribute attribute in ActiveSearch.SearchFiles)
		{
			newInstance.SearchFiles.Add(new TextAttribute(attribute.Text, attribute.Used));
		}

		ViewModel.SearchInstances.Add(newInstance);

		SetActiveTab(newInstance);
	}

	#endregion

	#region Preview Toolbar

	private void CommandSaveFile_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
	{
		string filePath = ((FileHit)dataGridFileList.SelectedItem).Path;

		if (File.Exists(filePath) && ViewModel.FileEdited)
		{
			try
			{
				using StreamWriter sw = new(filePath, false, ViewModel.FileEncoding.GetEncoding);

				if (ViewModel.PreviewLines.Count > 1 || ViewModel.PreviewLines[0].Text.Length > 0) // No new line in empty file
				{
					sw.NewLine = ViewModel.FileEncoding.GetNewLineString;
					foreach (Line l in ViewModel.PreviewLines)
					{
						sw.WriteLine(l.Text);
					}
				}
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message, "Error Saving File", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		UpdatePreview();
	}
	private void CommandSaveFile_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = ViewModel.FileEdited;
	}

	private void CommandEdit_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = dataGridFileList.SelectedItems.Count == 1 && !ActiveSearch.ShowOnlyHits && !ViewModel.FileEdited && !((FileHit)dataGridFileList.SelectedItems[0]).IsFolder;
	}

	private void CommandFirstHit_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
	{
		MoveToFirstHit();
	}
	private void CommandFirstHit_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = firstHit != -1 && ViewModel.CurrentHit > firstHit && !ViewModel.FileEdited;
	}

	private void CommandPreviousHit_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
	{
		MoveToPreviousHit();
	}
	private void CommandPreviousHit_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = firstHit != -1 && ViewModel.CurrentHit > firstHit && !ViewModel.FileEdited;
	}

	private void CommandCurrentHit_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
	{
		CenterOnLine(ViewModel.CurrentHit);
	}
	private void CommandCurrentHit_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = ViewModel.CurrentHit != -1 && !ViewModel.FileEdited;
	}

	private void CommandNextHit_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
	{
		MoveToNextHit();
	}
	private void CommandNextHit_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = lastHit != -1 && ViewModel.CurrentHit < lastHit && !ViewModel.FileEdited;
	}

	private void CommandLastHit_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
	{
		MoveToLastHit();
	}
	private void CommandLastHit_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = lastHit != -1 && ViewModel.CurrentHit < lastHit && !ViewModel.FileEdited;
	}

	#endregion

	#region Find Toolbar

	private void CommandFind_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
	{
		SearchPanel.Visibility = Visibility.Visible;
		SearchBox.Focus();
	}

	private void CommandFindNext_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
	{
		if (SearchPanel.Visibility != Visibility.Visible)
		{
			SearchPanel.Visibility = Visibility.Visible;
			SearchBox.Focus();
		}
		else
		{
			ProcessSearchResult(Preview.SearchNext(SearchBox.Text, MatchCase.IsChecked == true));
		}
	}
	private void CommandFindNext_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = (SearchBox.Text != "" && Preview.Lines.Count > 0) || SearchPanel.Visibility != Visibility.Visible;
	}

	private void CommandFindPrevious_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
	{
		ProcessSearchResult(Preview.SearchPrevious(SearchBox.Text, MatchCase.IsChecked == true));
	}
	private void CommandFindPrevious_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = SearchBox.Text != "" && Preview.Lines.Count > 0;
	}

	private void CommandCloseFind_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
	{
		SearchPanel.Visibility = Visibility.Collapsed;
	}

	#endregion

	#endregion

}
