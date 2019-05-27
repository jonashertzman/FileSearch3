using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace FileSearch
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{

		#region Members

		MainWindowViewModel ViewModel { get; set; } = new MainWindowViewModel();
		DispatcherTimer updatePrevirewTimer = new DispatcherTimer();

		int firstHit = -1;
		int lastHit = -1;

		int standardColumnCount = 0;

		#endregion

		#region Constructor

		public MainWindow()
		{
			InitializeComponent();


			DataContext = ViewModel;

			SearchPanel.Visibility = Visibility.Collapsed;

			updatePrevirewTimer.Interval = new TimeSpan(0, 0, 0, 0, 50);
			updatePrevirewTimer.Tick += UpdatePrevirewTimer_Tick;

			standardColumnCount = dataGridFileList.Columns.Count;

			UpdateStats();
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
			AppSettings.ReadSettingsFromDisk();

			this.Left = AppSettings.PositionLeft;
			this.Top = AppSettings.PositionTop;
			this.Width = AppSettings.Width;
			this.Height = AppSettings.Height;

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
				for (int i = s.SearchPhrases.Count - 1; i >= 0; i--)
				{
					if (string.IsNullOrEmpty(s.SearchPhrases[i].Text))
					{
						s.SearchPhrases.RemoveAt(i);
					}
				}
				for (int i = s.SearchDirectories.Count - 1; i >= 0; i--)
				{
					if (string.IsNullOrEmpty(s.SearchDirectories[i].Text))
					{
						s.SearchDirectories.RemoveAt(i);
					}
				}
				for (int i = s.SearchFiles.Count - 1; i >= 0; i--)
				{
					if (string.IsNullOrEmpty(s.SearchFiles[i].Text))
					{
						s.SearchFiles.RemoveAt(i);
					}
				}
			}
		}

		private void AddNewSearch()
		{
			SearchInstance newInstance = new SearchInstance();
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
			while (dataGridFileList.Columns.Count > standardColumnCount)
			{
				dataGridFileList.Columns.RemoveAt(standardColumnCount);
			}

			foreach (string s in ActiveSearch.StoredSearchPhrases)
			{
				ActiveSearch.PhraseSums[s] = 0;
			}

			int filesFound = 0;

			foreach (FileHit f in ActiveSearch.FilesWithHits)
			{
				if (ActiveSearch.FindAllPhrases ? f.AllPrasesHit(ActiveSearch.CaseSensitive) : f.AnyPhraseHit(ActiveSearch.CaseSensitive))
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

			int i = 0;
			foreach (string s in ActiveSearch.StoredSearchPhrases)
			{
				dataGridFileList.Columns.Add(new DataGridTextColumn()
				{
					Header = $"{s} ({ActiveSearch.PhraseSums[s]})",
					Binding = ActiveSearch.CaseSensitive ? new Binding($"PhraseHitsList[{i}].CaseSensitiveCount") : new Binding($"PhraseHitsList[{i}].Count"),
					CellStyle = (Style)FindResource("RightAlignedCell")
				});
				i++;
			}

			ActiveSearch.FileCountStatus = ActiveSearch.FilesSearched == 0 ? $"{filesFound} files found" : $"{ filesFound} files found in {ActiveSearch.FilesSearched} searched";
		}

		private void UpdatePreview()
		{
			Debug.Print("UpdatePreview");

			Mouse.OverrideCursor = Cursors.Wait;

			ObservableCollection<Line> Lines = new ObservableCollection<Line>();
			List<FileHit> previewFiles = new List<FileHit>();

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
				string[] allLines = new string[0];
				string allText = "";
				int lineNumber = 1;

				if (Directory.Exists(currentFile.Path))
				{
					Lines.Add(new Line() { Type = TextState.Header, Text = currentFile.Path });
					Lines.Add(new Line() { Type = TextState.SurroundSpacing, Text = "[FOLDER]" });
				}
				else
				{
					try
					{
						ViewModel.FileEncoding = Unicode.GetEncoding(currentFile.Path);
						ViewModel.FileDirty = false;

						if (previewFiles.Count > 1)
						{
							Lines.Add(new Line() { Type = TextState.Header, Text = currentFile.Path });
						}

						if (ActiveSearch.RegexSearch)
						{
							allText = File.ReadAllText(currentFile.Path, ViewModel.FileEncoding.Type);
							if (!(allText.EndsWith("\r\n") || allText.EndsWith("\r") || allText.EndsWith("\n")))
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
						List<RegexHit> regexHits = new List<RegexHit>();
						foreach (string searchPhrase in ActiveSearch.StoredSearchPhrases)
						{
							Match match = Regex.Match(allText, searchPhrase, ActiveSearch.CaseSensitive ? RegexOptions.Multiline : RegexOptions.Multiline | RegexOptions.IgnoreCase);
							while (match.Success)
							{
								regexHits.Add(new RegexHit(match.Index, match.Length));
								match = match.NextMatch();
							}
						}

						int lineSourceIndex = 0; // Start index for the current line including all new line characters in the source file.

						MatchCollection newLines = Regex.Matches(allText, "(\r\n|\r|\n)");

						foreach (Match newLine in newLines)
						{
							Line previewLine = new Line();
							previewLine.Text = allText.Substring(lineSourceIndex, newLine.Index - lineSourceIndex);
							previewLine.CurrentFile = currentFile.Path;
							previewLine.LineNumber = lineNumber++;
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
					else
					{
						foreach (string line in allLines)
						{
							Line previewLine = new Line();
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
			ObservableCollection<Line> newLines = new ObservableCollection<Line>();
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
			MoveToPrevoiusHit();
		}

		private void MoveToPrevoiusHit()
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
				SearchBox.Background = new SolidColorBrush(Colors.White);
				CenterOnLine(result);
			}
			else
			{
				SearchBox.Background = new SolidColorBrush(Colors.Tomato);
			}
		}

		private bool ValidateRegex()
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
							MessageBox.Show(p.Text + "  is not a valid regular expression", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
							return false;
						}
					}
				}
			}
			return true;
		}

		#endregion

		#region Events

		private void Window_Closed(object sender, EventArgs e)
		{
			SaveSettings();
		}

		private void Window_Initialized(object sender, EventArgs e)
		{
			LoadSettings();
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

			updatePrevirewTimer.Start();
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

		private void UpdatePrevirewTimer_Tick(object sender, EventArgs e)
		{
			updatePrevirewTimer.Stop();
			UpdatePreview();
		}

		private void DataGridFileList_Sorting(object sender, DataGridSortingEventArgs e)
		{
			updatePrevirewTimer.Start();
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
			int lines = SystemParameters.WheelScrollLines * e.Delta / 120;
			VerticalScrollbar.Value -= lines;
		}

		private void DataGridFileList_RowDoubleClick(object sender, MouseButtonEventArgs e)
		{
			Process p = new Process();
			p.StartInfo.FileName = ((FileHit)((DataGridRow)sender).Item).Path;
			p.StartInfo.ErrorDialog = true;
			p.Start();
		}

		private void BrowseDirectoryButton_Click(object sender, RoutedEventArgs e)
		{
			TextAttribute t = (sender as Button).DataContext as TextAttribute;

			System.Windows.Forms.FolderBrowserDialog d = new System.Windows.Forms.FolderBrowserDialog();
			if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				if (t == null)
				{
					ActiveSearch.SearchDirectories.Add(new TextAttribute(d.SelectedPath));
				}
				else
				{
					t.Text = d.SelectedPath;
				}
			}
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

		private void CommnadOptions_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
		{
			// Store existing settings data in case the changes are canceled.
			var oldFont = ViewModel.Font;
			var oldFontSize = ViewModel.FontSize;
			var oldTabSize = ViewModel.TabSize;
			var oldNormalForeground = ViewModel.NormalForeground;
			var oldNormalBackground = ViewModel.NormalBackground;
			var oldHitForeground = ViewModel.HitForeground;
			var oldHitBackground = ViewModel.HitBackground;
			var oldHeaderForeground = ViewModel.HeaderForeground;
			var oldHeaderBackground = ViewModel.HeaderBackground;
			var oldIgnoredFiles = new ObservableCollection<TextAttribute>(ViewModel.IgnoredFiles);
			var oldIgnoredDirectories = new ObservableCollection<TextAttribute>(ViewModel.IgnoredDirectories);

			OptionsWindow optionsWindow = new OptionsWindow() { DataContext = ViewModel, Owner = this };
			optionsWindow.ShowDialog();

			if (optionsWindow.DialogResult == true)
			{
				SaveSettings();
			}
			else
			{
				// Options window was canceled, revert to old settings.
				ViewModel.Font = oldFont;
				ViewModel.FontSize = oldFontSize;
				ViewModel.TabSize = oldTabSize;
				ViewModel.NormalForeground = oldNormalForeground;
				ViewModel.NormalBackground = oldNormalBackground;
				ViewModel.HitForeground = oldHitForeground;
				ViewModel.HitBackground = oldHitBackground;
				ViewModel.HeaderForeground = oldHeaderForeground;
				ViewModel.HeaderBackground = oldHeaderBackground;
				ViewModel.IgnoredFiles = new ObservableCollection<TextAttribute>(oldIgnoredFiles);
				ViewModel.IgnoredDirectories = new ObservableCollection<TextAttribute>(oldIgnoredDirectories);
			}
		}

		private void CommandAbout_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
		{
			AboutWindow aboutWindow = new AboutWindow() { Owner = this };
			aboutWindow.ShowDialog();
		}

		private void CommandRenameTab_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			RenameTabWindow renameTabWindow = new RenameTabWindow() { TabName = ActiveSearch.Name };
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
			ProcessStartInfo pfi = new ProcessStartInfo("Explorer.exe", args);
			Process.Start(pfi);
		}

		private void CommandOpenContainingFolder_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = dataGridFileList.SelectedItems.Count == 1;
		}

		private void CommandCopyPathToClipboard_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			Clipboard.SetText(Path.GetFullPath(((FileHit)dataGridFileList.SelectedItem).Path));
		}

		private void CommandCopyPathToClipboard_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = dataGridFileList.SelectedItems.Count == 1;
		}

		#endregion

		#region Main Toolbar

		private void CommandStartSearch_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
		{
			if (!ValidateRegex())
			{
				return;
			}

			dataGridFileList.Focus();

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
			SearchInstance newInstance = new SearchInstance();

			newInstance.CaseSensitive = ActiveSearch.CaseSensitive;
			newInstance.RegexSearch = ActiveSearch.RegexSearch;

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

		}
		private void CommandSaveFile_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = false;
		}

		private void CommandEdit_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
		{

		}
		private void CommandEdit_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = false;
		}

		private void CommandFirstHit_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
		{
			MoveToFirstHit();
		}
		private void CommandFirstHit_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = firstHit != -1 && ViewModel.CurrentHit > firstHit;
		}

		private void CommandPreviousHit_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
		{
			MoveToPrevoiusHit();
		}
		private void CommandPreviousHit_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = firstHit != -1 && ViewModel.CurrentHit > firstHit;
		}

		private void CommandCurrentHit_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
		{
			CenterOnLine(ViewModel.CurrentHit);
		}
		private void CommandCurrentHit_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = ViewModel.CurrentHit != -1;
		}

		private void CommandNextHit_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
		{
			MoveToNextHit();
		}
		private void CommandNextHit_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = lastHit != -1 && ViewModel.CurrentHit < lastHit;
		}

		private void CommandLastHit_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
		{
			MoveToLastHit();
		}
		private void CommandLastHit_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = lastHit != -1 && ViewModel.CurrentHit < lastHit;
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
}
