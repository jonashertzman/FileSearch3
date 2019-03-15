﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace FileSearch
{
	public class SearchInstance : INotifyPropertyChanged
	{

		#region Menmbers

		MainWindow mainWindow;

		BackgroundWorker backgroundWorker = new BackgroundWorker();

		DateTime lastStatusUpdateTime = DateTime.UtcNow;
		DateTime startTime = new DateTime();
		DateTime endTime = new DateTime();

		string currentRoot;

		//List<string> uppercaseIgnoreDirectories = new List<string>();
		//List<string> uppercaseIgnoreFiles = new List<string>();
		//List<string> uppercaseSearchFiles = new List<string>();

		#endregion

		#region Constructor

		public SearchInstance()
		{
			Name = "[New Search]";


			backgroundWorker.DoWork += BackgroundWorker_DoWork;

			backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
		}

		#endregion

		#region Overrides

		public override string ToString()
		{
			return name;

		}

		#endregion

		#region Properties
		private bool searchInProgress;
		public bool SearchInProgress
		{
			get { return searchInProgress; }
			private set { searchInProgress = value; OnPropertyChanged(nameof(SearchInProgress)); }
		}

		string name;
		public string Name
		{
			get { return name; }
			set { name = value; OnPropertyChanged(nameof(Name)); }
		}

		bool isSelected;
		public bool IsSelected
		{
			get { return isSelected; }
			set { isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
		}

		bool regexSearch;
		public bool RegexSearch
		{
			get { return regexSearch; }
			set { regexSearch = value; OnPropertyChanged(nameof(RegexSearch)); }
		}

		ObservableCollection<TextAttribute> searchPhrases = new ObservableCollection<TextAttribute>();
		public ObservableCollection<TextAttribute> SearchPhrases
		{
			get { return searchPhrases; }
			set { searchPhrases = value; OnPropertyChanged(nameof(SearchPhrases)); }
		}

		ObservableCollection<TextAttribute> searchDirectories = new ObservableCollection<TextAttribute>();
		public ObservableCollection<TextAttribute> SearchDirectories
		{
			get { return searchDirectories; }
			set { searchDirectories = value; OnPropertyChanged(nameof(SearchDirectories)); }
		}

		ObservableCollection<TextAttribute> searchFiles = new ObservableCollection<TextAttribute>();
		public ObservableCollection<TextAttribute> SearchFiles
		{
			get { return searchFiles; }
			set { searchFiles = value; OnPropertyChanged(nameof(SearchFiles)); }
		}

		ObservableCollection<FileHit> filesWithHits = new ObservableCollection<FileHit>();
		public ObservableCollection<FileHit> FilesWithHits
		{
			get { return filesWithHits; }
			set { filesWithHits = value; OnPropertyChanged(nameof(FilesWithHits)); }
		}

		int searchedFilesCount = 0;
		public int SearchedFilesCount
		{
			get { return searchedFilesCount; }
			set { searchedFilesCount = value; OnPropertyChanged(nameof(SearchedFilesCount)); }
		}

		string statusText;
		public string StatusText
		{
			get { return statusText; }
			set { statusText = value; OnPropertyChanged(nameof(StatusText)); }
		}

		string fileCountStatus;
		public string FileCountStatus
		{
			get { return fileCountStatus; }
			set { fileCountStatus = value; OnPropertyChanged(nameof(FileCountStatus)); }
		}

		int progress;
		public int Progress
		{
			get { return progress; }
			set { progress = value; OnPropertyChanged(nameof(Progress)); }
		}

		public bool CaseSensitive { get; internal set; }

		private List<FileHit> SearchResults = new List<FileHit>();

		#endregion

		#region Methods

		private void FindFiles(string searchDirectory)
		{
			if (SearchInProgress)
			{
				IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
				IntPtr findHandle = FindFirstFile(Path.Combine(searchDirectory, "*"), out WIN32_FIND_DATA findData);

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
									if (SearchPhrases.Count == 0)
									{
										foreach (TextAttribute s in searchFiles)
										{
											if (WildcardCompare(uppercaseFileName, s.UppercaseText, false))
											{
												SearchResults.Add(new FileHit(newPath));
											}
										}
									}
									UpdateStatus(newPath);
									FindFiles(newPath);
								}
							}
						}

						// File
						else
						{
							if (!FileIsIgnored(uppercaseFileName))
							{
								foreach (TextAttribute s in searchFiles)
								{
									if (WildcardCompare(uppercaseFileName, s.UppercaseText, false))
									{
										if (SearchPhrases.Count == 0)
										{
											SearchResults.Add(new FileHit(newPath));
											break;
										}
										UpdateStatus(newPath);

										SearchInFile(newPath);
										break;
									}
								}
							}
						}
					}
					while (FindNextFile(findHandle, out findData) && SearchInProgress);
				}

				FindClose(findHandle);
			}
		}

		private bool FileIsIgnored(string fileName)
		{
			foreach (TextAttribute s in AppSettings.IgnoredFiles)
			{
				if (WildcardCompare(fileName, s.UppercaseText, false))
				{
					return true;
				}
			}
			return false;
		}

		private bool DirectoryIsIgnored(string directory)
		{
			foreach (TextAttribute s in AppSettings.IgnoredFolders)
			{
				if (WildcardCompare(directory, s.UppercaseText, false))
				{
					return true;
				}
			}
			return false;
		}

		private void UpdateStatus(string statusText, bool finalUpdate = false)
		{
			const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ_";
			int percentageComplete;

			if (finalUpdate || (DateTime.UtcNow - lastStatusUpdateTime).TotalMilliseconds >= 300)
			{
				if (!finalUpdate)
				{
					char firstLetter = Char.ToUpper(statusText[currentRoot.Length]);
					int index = alphabet.IndexOf(firstLetter);
					percentageComplete = index == -1 ? index : (int)((float)(index / (float)alphabet.Length) * 100.0);
				}
				else
				{
					percentageComplete = 0;
				}

				mainWindow.Dispatcher.BeginInvoke(mainWindow.searchProgressUpdateDelegate, new Object[] { this, SearchResults, statusText, percentageComplete });
				lastStatusUpdateTime = DateTime.UtcNow;
			}
		}

		private void SearchInFile(string path)
		{
			try
			{
				FileHit currentHit = null;
				SearchedFilesCount++;

				if (RegexSearch)
				{
					string allText = File.ReadAllText(path);
					foreach (TextAttribute searchPhrase in SearchPhrases)
					{
						Match match = Regex.Match(allText, searchPhrase.Text, RegexOptions.Multiline | RegexOptions.IgnoreCase);
						while (match.Success)
						{
							if (currentHit == null)
							{
								currentHit = new FileHit(path);
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

					foreach (TextAttribute searchPhrase in SearchPhrases)
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
								currentHit = new FileHit(path);
							}
							currentHit.AddPhraseHit(searchPhrase.Text, allText.IndexOf(searchPhrase.Text, i, StringComparison.Ordinal) == i);

							i += searchPhrase.Text.Length;
						}
						while (true);
					}
				}

				if (currentHit != null)
				{
					SearchResults.Add(currentHit);
				}
			}
			catch (Exception)
			{
				//LogedItems.Add(e.Message);
			}
		}

		internal static bool WildcardCompare(string compare, string wildString, bool ignoreCase)
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

		internal void Search(MainWindow window)
		{
			mainWindow = window;

			if (searchFiles.Count == 0)
			{
				searchFiles.Add(new TextAttribute("*"));
			}

			SearchInProgress = true;
			FilesWithHits.Clear();
			SearchResults.Clear();
			//LogedItems.Clear();
			SearchedFilesCount = 0;

			backgroundWorker.RunWorkerAsync();
		}

		internal void CancelSearch()
		{
			searchInProgress = false;
		}

		#endregion

		#region Events

		void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			UpdateStatus("Done", true);
			SearchInProgress = false;
		}

		void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			startTime = DateTime.UtcNow;

			foreach (TextAttribute s in SearchDirectories)
			{
				currentRoot = s.Text + (s.Text.EndsWith("\\") ? "" : "\\");
				FindFiles(currentRoot);
			}

			endTime = DateTime.UtcNow;
		}

		#endregion

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		#endregion

		#region API Imports

		public const int MAX_PATH = 260;
		public const int MAX_ALTERNATE = 14;
		public const long MAXDWORD = 0xffffffff;
		public const int FIND_FIRST_EX_LARGE_FETCH = 2;

		public enum FINDEX_INFO_LEVELS
		{
			FindExInfoStandard = 0,
			FindExInfoBasic = 1
		}

		public enum FINDEX_SEARCH_OPS
		{
			FindExSearchNameMatch = 0,
			FindExSearchLimitToDirectories = 1,
			FindExSearchLimitToDevices = 2
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct FILETIME
		{
			public uint dwLowDateTime;
			public uint dwHighDateTime;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public struct WIN32_FIND_DATA
		{
			public FileAttributes dwFileAttributes;
			public FILETIME ftCreationTime;
			public FILETIME ftLastAccessTime;
			public FILETIME ftLastWriteTime;
			public int nFileSizeHigh;
			public int nFileSizeLow;
			public int dwReserved0;
			public int dwReserved1;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
			public string cFileName;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_ALTERNATE)]
			public string cAlternate;
		}

		[DllImport("kernel32", CharSet = CharSet.Auto)]
		public static extern IntPtr FindFirstFile(string lpFileName, out WIN32_FIND_DATA lpFindFileData);

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern IntPtr FindFirstFileEx(string lpFileName, FINDEX_INFO_LEVELS fInfoLevelId, out WIN32_FIND_DATA lpFindFileData, FINDEX_SEARCH_OPS fSearchOp, IntPtr lpSearchFilter, int dwAdditionalFlags);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		public static extern bool FindNextFile(IntPtr hFindFile, out WIN32_FIND_DATA lpFindFileData);

		[DllImport("kernel32.dll")]
		static extern bool FindClose(IntPtr hFindFile);

		#endregion

	}
}
