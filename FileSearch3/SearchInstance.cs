using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace FileSearch
{
	public class SearchInstance : INotifyPropertyChanged
	{

		#region Constructor

		public SearchInstance()
		{
			Name = "[New Search]";
		}

		#endregion

		DateTime lastStatusUpdateTime = DateTime.UtcNow;
		BackgroundWorker backgroundWorker = new BackgroundWorker();

		DateTime startTime = new DateTime();
		DateTime endTime = new DateTime();

		bool searchInProgress;
		//List<string> uppercaseIgnoreDirectories = new List<string>();
		//List<string> uppercaseIgnoreFiles = new List<string>();
		//List<string> uppercaseSearchFiles = new List<string>();

		#region Overrides

		public override string ToString()
		{
			return name;
		}

		#endregion

		#region Properties

		string name;
		public string Name
		{
			get { return name; }
			set { name = value; OnPropertyChanged("Name"); }
		}

		bool isSelected;
		public bool IsSelected
		{
			get { return isSelected; }
			set { isSelected = value; OnPropertyChanged("IsSelected"); }
		}

		bool regexSearch;
		public bool RegexSearch
		{
			get { return regexSearch; }
			set { regexSearch = value; OnPropertyChanged("RegexSearch"); }
		}

		ObservableCollection<TextAttribute> searchPhrases = new ObservableCollection<TextAttribute>();
		public ObservableCollection<TextAttribute> SearchPhrases
		{
			get { return searchPhrases; }
			set { searchPhrases = value; OnPropertyChanged("SearchPhrases"); }
		}

		ObservableCollection<TextAttribute> searchDirectories = new ObservableCollection<TextAttribute>();
		public ObservableCollection<TextAttribute> SearchDirectories
		{
			get { return searchDirectories; }
			set { searchDirectories = value; OnPropertyChanged("SearchDirectories"); }
		}

		ObservableCollection<TextAttribute> searchFiles = new ObservableCollection<TextAttribute>();
		public ObservableCollection<TextAttribute> SearchFiles
		{
			get { return searchFiles; }
			set { searchFiles = value; OnPropertyChanged("SearchFiles"); }
		}

		ObservableCollection<FileHit> filesWithHits = new ObservableCollection<FileHit>();
		public ObservableCollection<FileHit> FilesWithHits
		{
			get { return filesWithHits; }
			set { filesWithHits = value; OnPropertyChanged("FilesWithHits"); }
		}

		int searchedFilesCount = 0;
		public int SearchedFilesCount
		{
			get { return searchedFilesCount; }
			set { searchedFilesCount = value; OnPropertyChanged("SearchedFilesCount"); }
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

		#region Methods

		private void FindFiles(string searchDirectory)
		{
			if (searchInProgress)
			{
				IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
				WIN32_FIND_DATA findData;
				IntPtr findHandle = FindFirstFile(Path.Combine(searchDirectory, "*"), out findData);
				//IntPtr findHandle = FindFirstFileEx(Path.Combine(searchDirectory, "*"), FINDEX_INFO_LEVELS.FindExInfoBasic, out findData, FINDEX_SEARCH_OPS.FindExSearchNameMatch, IntPtr.Zero, FIND_FIRST_EX_LARGE_FETCH);

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
												FilesWithHits.Add(new FileHit(newPath));
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
											FilesWithHits.Add(new FileHit(newPath));
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
					while (FindNextFile(findHandle, out findData) && searchInProgress);
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

		private void UpdateStatus(string statusText)
		{
			const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ_";

			if ((DateTime.UtcNow - lastStatusUpdateTime).TotalMilliseconds >= 100)
			{
				char firstLetter = Char.ToUpper(statusText[currentRoot.Length]);
				int index = alphabet.IndexOf(firstLetter);
				int percentageComplete = index == -1 ? index : (int)((float)(index / (float)alphabet.Length) * 100.0);
				//mainForm.Invoke(mainForm.statusUpdateDelegate, new Object[] { this, statusText, percentageComplete });
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
					FilesWithHits.Add(currentHit);
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

		internal void Search()
		{

			if (searchFiles.Count == 0)
			{
				searchFiles.Add(new TextAttribute("*"));
			}

			searchInProgress = true;
			FilesWithHits.Clear();
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
			//mainForm.CompleteSearch(this);
		}

		string currentRoot;

		void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			startTime = DateTime.UtcNow;

			foreach (TextAttribute s in SearchDirectories)
			{
				currentRoot = s + (s.Text.EndsWith("\\") ? "" : "\\");
				FindFiles(currentRoot);
			}

			endTime = DateTime.UtcNow;
		}

		#endregion

	}
}
