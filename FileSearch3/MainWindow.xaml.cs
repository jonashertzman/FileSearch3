using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace FileSearch
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		MainWindowViewModel ViewModel { get; set; } = new MainWindowViewModel();

		#region Constructor

		public MainWindow()
		{
			InitializeComponent();

			DataContext = ViewModel;

			SearchPanel.Visibility = Visibility.Collapsed;
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

		internal void CompleteSearch(SearchInstance searchInstance)
		{
			foreach (FileHit f in searchInstance.SearchResults)
			{
				searchInstance.FilesWithHits.Add(f);
			}
		}

		private void UpdatePreview()
		{

			ObservableCollection<Line> Lines = new ObservableCollection<Line>();

			Debug.Print("\n\n");

			foreach (FileHit f in dataGridFileList.Items)
			{
				if (f.Selected)
				{
					Debug.Print(f.Path);

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
						foreach (string line in allLines)
						{
							Line previewLine = new Line();
							previewLine.Text = line;
							previewLine.CurrentFile = f.Path;

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
									//previewLine.TextSegments.Add(new TextSegment(hitIndex, phrase.Text.Length));
									hitIndex += phrase.Text.Length;
									previewLine.Type = TextState.Hit;
								}
							}

							Lines.Add(previewLine);
						}
					}

				}
			}

			Preview.Init();

			ViewModel.PreviewLines = Lines;
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

		private void CommandStartSearch_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
		{
			ActiveSearch.Search(this);
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

		private void CommandEdit_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
		{

		}
		private void CommandEdit_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
		{

		}

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

	}
}
