using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
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

		#endregion

		#region Constructor

		public MainWindow()
		{
			InitializeComponent();


			DataContext = ViewModel;

			SearchPanel.Visibility = Visibility.Collapsed;

			updatePrevirewTimer.Interval = new TimeSpan(500);
			updatePrevirewTimer.Tick += UpdatePrevirewTimer_Tick;
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
				ViewModel.SearchInstances.Add(new SearchInstance());
			}

			SetActiveTab(ViewModel.SearchInstances[0]);
		}

		private void SaveSettings()
		{
			AppSettings.PositionLeft = this.Left;
			AppSettings.PositionTop = this.Top;
			AppSettings.Width = this.Width;
			AppSettings.Height = this.Height;
			AppSettings.WindowState = this.WindowState;

			AppSettings.WriteSettingsToDisk();
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
				s.IsSelected = s == searchInstance;
			}
		}

		private void UpdatePreview()
		{
			Debug.Print("UpdatePreview");

			ObservableCollection<Line> Lines = new ObservableCollection<Line>();
			firstHit = -1;
			lastHit = -1;

			foreach (FileHit f in dataGridFileList.Items)
			{
				if (f.Selected)
				{
					string[] allLines = new string[0];
					string allText = "";

					try
					{
						if (File.Exists(f.Path))
						{
							ViewModel.FileEncoding = Unicode.GetEncoding(f.Path);
							ViewModel.FileDirty = false;

							if (dataGridFileList.SelectedItems.Count > 1)
							{
								Lines.Add(new Line() { Type = TextState.Header, Text = f.Path });
							}

							if (ActiveSearch.RegexSearch)
							{
								allText = File.ReadAllText(f.Path, ViewModel.FileEncoding.Type);
								if (!(allText.EndsWith("\r\n") || allText.EndsWith("\r") || allText.EndsWith("\n")))
								{
									allText += ViewModel.FileEncoding.GetNewLineString;
								}
							}
							else
							{
								allLines = File.ReadAllLines(f.Path, ViewModel.FileEncoding.Type);
							}
						}
					}
					catch (IOException e)
					{
						MessageBox.Show(e.Message, e.GetType().Name);
						continue;
					}

					if (ActiveSearch.RegexSearch)
					{
						List<RegexHit> regexHits = new List<RegexHit>();
						foreach (TextAttribute searchPhrase in ActiveSearch.SearchPhrases)
						{
							Match match = Regex.Match(allText, searchPhrase.Text, ActiveSearch.CaseSensitive ? RegexOptions.Multiline : RegexOptions.Multiline | RegexOptions.IgnoreCase);
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
							previewLine.CurrentFile = f.Path;

							int lineSourceLength = previewLine.Text.Length + newLine.Length; // Length of current line including all new line characters.

							foreach (RegexHit regexHit in regexHits)
							{
								if (regexHit.Start < lineSourceIndex + lineSourceLength && lineSourceIndex <= regexHit.Start + regexHit.Length)
								{
									previewLine.Type = TextState.Hit;
									int selectionStartIndex = Math.Max(regexHit.Start - lineSourceIndex, 0);
									int selectionEndIndex = Math.Min((regexHit.Start - lineSourceIndex) + regexHit.Length, previewLine.Text.Length);

									//previewLine.TextSegments.Add(new TextSegment(selectionStartIndex, selectionEndIndex - selectionStartIndex));
								}
							}
							Lines.Add(previewLine);

							lineSourceIndex += lineSourceLength;
						}
					}
					else
					{
						int lineNumber = 1;
						foreach (string line in allLines)
						{
							Line previewLine = new Line();
							bool[] hitCharacters = new bool[line.Length];
							previewLine.Text = line;
							previewLine.CurrentFile = f.Path;
							previewLine.LineNumber = lineNumber++;

							foreach (TextAttribute phrase in ActiveSearch.SearchPhrases)
							{
								int hitIndex = 0;
								while (true)
								{
									hitIndex = line.IndexOf(phrase.Text, hitIndex, ActiveSearch.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
									if (hitIndex == -1)
									{
										break;
									}

									for (int i = hitIndex; i < hitIndex + phrase.Text.Length; i++)
									{
										hitCharacters[i] = true;
									}

									hitIndex += phrase.Text.Length;
									previewLine.Type = TextState.Hit;
								}
							}

							if (previewLine.Type == TextState.Hit)
							{
								lastHit = Lines.Count;
								if (firstHit == -1)
								{
									firstHit = lastHit;
								}

								previewLine.TextSegments.Clear();

								int start = 0;
								for (int i = 1; i < line.Length; i++)
								{
									if (hitCharacters[start] == hitCharacters[i])
										continue;

									previewLine.TextSegments.Add(new TextSegment(line.Substring(start, i - start), hitCharacters[start] ? TextState.Hit : TextState.Normal));
									start = i;
								}
								previewLine.TextSegments.Add(new TextSegment(line.Substring(start, line.Length - start), hitCharacters[start] ? TextState.Hit : TextState.Normal));
							}

							Lines.Add(previewLine);
						}
					}

				}
			}

			Preview.Init();

			ViewModel.PreviewLines = Lines;

			MoveToFirstHit();
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
					CenterOnLine(i);
					return;
				}
			}
		}

		private void CenterOnLine(int i)
		{
			ViewModel.CurrentHit = i;

			int visibleLines = Preview.VisibleLines <= 0 ? (int)(Preview.ActualHeight / OneCharacter.ActualHeight) : Preview.VisibleLines;

			VerticalScrollbar.Value = i - (visibleLines / 2) + 1;
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

		private void ButtonNewSearchInstance_Click(object sender, RoutedEventArgs e)
		{
			AddNewSearch();
		}

		private void TabButton_Click(object sender, RoutedEventArgs e)
		{
			SetActiveTab((SearchInstance)((Button)e.Source).Tag);
		}

		private void DataGridFileList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			updatePrevirewTimer.Start();
		}

		private void UpdatePrevirewTimer_Tick(object sender, EventArgs e)
		{
			updatePrevirewTimer.Stop();
			UpdatePreview();
		}

		private void DataGridFileList_Sorting(object sender, DataGridSortingEventArgs e)
		{
			// Workaround to trigger update after sorting is done since there is no Sorted event.
			this.Dispatcher.BeginInvoke((Action)delegate ()
			{
				UpdatePreview();
			}, null);
		}

		private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
		{

		}

		private void MatchCase_Checked(object sender, RoutedEventArgs e)
		{

		}

		private void Preview_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
		{
			int lines = SystemParameters.WheelScrollLines * e.Delta / 120;
			VerticalScrollbar.Value -= lines;
		}

		#endregion

		#region Commands

		#region Menu

		private void CommandExit_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
		{
			this.Close();
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
			var oldIgnoredFolders = new ObservableCollection<TextAttribute>(ViewModel.IgnoredFolders);

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
				ViewModel.IgnoredFolders = new ObservableCollection<TextAttribute>(oldIgnoredFolders);
			}
		}

		private void CommandAbout_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
		{
			AboutWindow aboutWindow = new AboutWindow() { Owner = this };
			aboutWindow.ShowDialog();
		}

		#endregion

		#region Main Toolbar

		private void CommandStartSearch_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
		{
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
			ViewModel.SearchInstances.Add(new SearchInstance());
		}

		#endregion

		#region Preview Toolbar

		private void CommandSaveFile_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
		{

		}
		private void CommandSaveFile_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		private void CommandEdit_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
		{

		}
		private void CommandEdit_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
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

		}

		private void CommandFindNext_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
		{

		}
		private void CommandFindNext_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
		{

		}

		private void CommandFindPrevious_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
		{

		}
		private void CommandFindPrevious_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
		{

		}

		private void CommandCloseFind_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
		{

		}

		#endregion

		#endregion

	}
}
