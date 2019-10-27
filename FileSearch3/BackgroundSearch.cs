using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;

namespace FileSearch
{
	class BackgroundSearch
	{

		#region Members

		SearchInstance searchInstance;

		DateTime lastStatusUpdateTime = DateTime.UtcNow;
		DateTime startTime = new DateTime();
		DateTime endTime = new DateTime();

		List<TextAttribute> searchPhrases = new List<TextAttribute>();
		List<TextAttribute> searchDirectories = new List<TextAttribute>();
		List<TextAttribute> searchFiles = new List<TextAttribute>();

		List<string> uppercaseIgnoreDirecrories = new List<string>();
		List<string> uppercaseIgnoreFiles = new List<string>();

		bool regexSearch;

		string currentRoot;

		List<FileHit> searchResults = new List<FileHit>();
		List<string> searchErrors = new List<string>();
		List<string> searchIgnores = new List<string>();

		BackgroundWorker backgroundWorker = new BackgroundWorker();

		bool abortPosted = false;

		int searchedFilesCount = 0;

		#endregion

		#region Constructor

		public BackgroundSearch(SearchInstance searchInstance)
		{
			this.searchInstance = searchInstance;

			backgroundWorker.DoWork += BackgroundWorker_DoWork;
			backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;

			regexSearch = searchInstance.RegexSearch;

			foreach (TextAttribute attribute in searchInstance.SearchPhrases)
			{
				if (!string.IsNullOrEmpty(attribute.Text) && attribute.Used)
				{
					searchPhrases.Add(new TextAttribute(attribute.Text));
				}
			}

			foreach (TextAttribute attribute in searchInstance.SearchDirectories)
			{
				if (!string.IsNullOrEmpty(attribute.Text) && attribute.Used)
				{
					searchDirectories.Add(new TextAttribute(attribute.Text));
				}
			}

			foreach (TextAttribute attribute in searchInstance.SearchFiles)
			{
				if (!string.IsNullOrEmpty(attribute.Text) && attribute.Used)
				{
					searchFiles.Add(new TextAttribute(attribute.Text));
				}
			}
			if (searchFiles.Count == 0)
			{
				searchFiles.Add(new TextAttribute("*"));
			}

			foreach (TextAttribute attribute in AppSettings.IgnoredFiles)
			{
				uppercaseIgnoreFiles.Add(attribute.UppercaseText);
			}

			foreach (TextAttribute attribute in AppSettings.IgnoredDirectories)
			{
				uppercaseIgnoreDirecrories.Add(attribute.UppercaseText);
			}

			Search();

		}

		#endregion

		#region Properties

		public bool SearchInProgress
		{
			get { return backgroundWorker.IsBusy; }
		}

		#endregion

		#region Methods

		private void Search()
		{
			backgroundWorker.RunWorkerAsync();
		}

		public void CancelSearch()
		{
			abortPosted = true;
		}

		private void FindFiles(string searchDirectory)
		{
			if (!abortPosted)
			{
				IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
				IntPtr findHandle = WinApi.FindFirstFile(Path.Combine(searchDirectory, "*"), out WIN32_FIND_DATA findData);

				string uppercaseFileName;
				string newPath;

				if (findHandle != INVALID_HANDLE_VALUE)
				{
					do
					{
						uppercaseFileName = findData.cFileName.ToUpper();
						newPath = Path.Combine(searchDirectory, findData.cFileName);

						// Directory
						if ((findData.dwFileAttributes & FileAttributes.Directory) != 0)
						{
							if (findData.cFileName != "." && findData.cFileName != "..")
							{
								if (!DirectoryIsIgnored(uppercaseFileName))
								{
									if (searchPhrases.Count == 0 && FileIsMatch(uppercaseFileName))
									{
										searchResults.Add(new FileHit(newPath, searchPhrases, findData, true));
									}
									UpdateStatus(newPath);
									FindFiles(newPath);
								}
							}
						}

						// File
						else
						{
							if (FileIsMatch(uppercaseFileName))
							{
								if (!FileIsIgnored(uppercaseFileName))
								{
									if (searchPhrases.Count == 0)
									{
										searchResults.Add(new FileHit(newPath, searchPhrases, findData));
									}
									UpdateStatus(newPath);

									SearchInFile(newPath, findData);
								}
								else
								{
									searchIgnores.Add(newPath);
								}
							}
						}
					}
					while (WinApi.FindNextFile(findHandle, out findData) && !abortPosted);
				}

				WinApi.FindClose(findHandle);
			}
		}

		private bool FileIsMatch(string fileName)
		{
			foreach (TextAttribute s in searchFiles)
			{
				if (WildcardCompare(fileName, s.UppercaseText, false))
				{
					return true;
				}
			}
			return false;
		}

		private bool FileIsIgnored(string fileName)
		{
			foreach (string s in uppercaseIgnoreFiles)
			{
				if (WildcardCompare(fileName, s, false))
				{
					return true;
				}
			}
			return false;
		}

		private bool DirectoryIsIgnored(string directory)
		{
			foreach (string s in uppercaseIgnoreDirecrories)
			{
				if (WildcardCompare(directory, s, false))
				{
					return true;
				}
			}
			return false;
		}

		private void UpdateStatus(string currentPath, bool finalUpdate = false)
		{
			if (finalUpdate || (DateTime.UtcNow - lastStatusUpdateTime).TotalMilliseconds >= 100)
			{
				const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ_";
				int percentageComplete;

				string status;
				if (currentPath != null)
				{
					char firstLetter = Char.ToUpper(currentPath[currentRoot.Length]);
					int index = alphabet.IndexOf(firstLetter);
					percentageComplete = index == -1 ? index : (int)((float)(index / (float)alphabet.Length) * 100.0);

					status = currentPath;
				}
				else
				{
					percentageComplete = 0;

					status = TimeSpanToShortString(endTime.Subtract(startTime));
				}

				searchInstance.mainWindow.Dispatcher.BeginInvoke(searchInstance.searchProgressUpdateDelegate, new Object[] { searchResults, searchErrors, searchIgnores, status, percentageComplete, searchedFilesCount });
				lastStatusUpdateTime = DateTime.UtcNow;
			}
		}

		private void SearchInFile(string path, WIN32_FIND_DATA findData)
		{
			try
			{
				FileHit currentHit = null;

				if (regexSearch)
				{
					string allText = File.ReadAllText(path);
					foreach (TextAttribute searchPhrase in searchPhrases)
					{
						Match match = Regex.Match(allText, searchPhrase.Text, RegexOptions.Multiline | RegexOptions.IgnoreCase);
						while (match.Success)
						{
							if (currentHit == null)
							{
								currentHit = new FileHit(path, searchPhrases, findData);
							}
							Match caseSensitiveMatch = Regex.Match(allText.Substring(match.Index), searchPhrase.Text);
							currentHit.AddPhraseHit(searchPhrase.Text, caseSensitiveMatch.Success && caseSensitiveMatch.Index == 0);
							match = match.NextMatch();
						}
					}
				}

				else
				{
					string allText = File.ReadAllText(path);

					foreach (TextAttribute searchPhrase in searchPhrases)
					{
						int i = 0;
						do
						{
							i = allText.IndexOf(searchPhrase.Text, i, StringComparison.OrdinalIgnoreCase);
							if (i == -1)
							{
								break;
							}
							if (currentHit == null)
							{
								currentHit = new FileHit(path, searchPhrases, findData);
							}
							currentHit.AddPhraseHit(searchPhrase.Text, allText.IndexOf(searchPhrase.Text, i, StringComparison.Ordinal) == i);

							i += searchPhrase.Text.Length;
						}
						while (true);
					}
				}

				if (currentHit != null)
				{
					searchResults.Add(currentHit);
				}
				searchedFilesCount++;
			}
			catch (Exception e)
			{
				searchErrors.Add(e.Message);
			}
		}

		private bool WildcardCompare(string compare, string wildString, bool ignoreCase)
		{
			if (ignoreCase)
			{
				wildString = wildString.ToUpper();
				compare = compare.ToUpper();
			}

			int wildStringLength = wildString.Length;
			int CompareLength = compare.Length;

			int wildMatched = wildStringLength;
			int compareBase = CompareLength;

			int wildPosition = 0;
			int comparePosition = 0;

			// Match until first wildcard '*'
			while (comparePosition < CompareLength && (wildPosition >= wildStringLength || wildString[wildPosition] != '*'))
			{
				if (wildPosition >= wildStringLength || (wildString[wildPosition] != compare[comparePosition] && wildString[wildPosition] != '?'))
				{
					return false;
				}

				wildPosition++;
				comparePosition++;
			}

			// Process wildcard
			while (comparePosition < CompareLength)
			{
				if (wildPosition < wildStringLength)
				{
					if (wildString[wildPosition] == '*')
					{
						wildPosition++;

						if (wildPosition == wildStringLength)
						{
							return true;
						}

						wildMatched = wildPosition;
						compareBase = comparePosition + 1;

						continue;
					}

					if (wildString[wildPosition] == compare[comparePosition] || wildString[wildPosition] == '?')
					{
						wildPosition++;
						comparePosition++;

						continue;
					}
				}

				wildPosition = wildMatched;
				comparePosition = compareBase++;
			}

			while (wildPosition < wildStringLength && wildString[wildPosition] == '*')
			{
				wildPosition++;
			}

			if (wildPosition < wildStringLength)
			{
				return false;
			}

			return true;
		}

		private string TimeSpanToShortString(TimeSpan timeSpan)
		{
			if (timeSpan.Hours > 0)
			{
				return timeSpan.Hours + "h " + timeSpan.Minutes + "m";
			}
			if (timeSpan.Minutes > 0)
			{
				return timeSpan.Minutes + "m " + timeSpan.Seconds + "s";
			}
			return timeSpan.Seconds + "." + timeSpan.Milliseconds.ToString().PadLeft(3, '0') + "s";
		}

		#endregion

		#region Events

		void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			startTime = DateTime.UtcNow;

			searchInstance.SearchInProgress = true;

			foreach (TextAttribute s in searchDirectories)
			{
				currentRoot = s.Text + (s.Text.EndsWith("\\") ? "" : "\\");
				FindFiles(currentRoot);
			}

			endTime = DateTime.UtcNow;
		}

		void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			searchInstance.SearchInProgress = false;
			UpdateStatus(null, true);
		}

		#endregion

	}
}
