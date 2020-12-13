using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text;

namespace FileSearch
{
	public class SearchInstance : INotifyPropertyChanged
	{

		#region Members

		BackgroundSearch backgroundSearch;

		internal MainWindow mainWindow;
		internal delegate void SearchProgressUpdateDelegate(List<FileHit> searchResults, List<string> searchErrors, List<string> searchIgnores, String statusText, int percentageComplete, int filesSearched);
		internal SearchProgressUpdateDelegate searchProgressUpdateDelegate;

		#endregion

		#region Constructor

		public SearchInstance()
		{
			Name = "[New Search]";

			searchProgressUpdateDelegate = new SearchProgressUpdateDelegate(SearchProgressUpdate);
		}

		#endregion

		#region Overrides

		public override string ToString()
		{
			return name;
		}

		#endregion

		#region Properties

		[IgnoreDataMember]
		public bool SearchInProgress
		{
			get { return backgroundSearch != null && backgroundSearch.SearchInProgress; }
			set { OnPropertyChanged(nameof(SearchInProgress)); }
		}

		string name;
		public string Name
		{
			get { return name; }
			set { name = value; OnPropertyChanged(nameof(Name)); }
		}

		public bool Renamed { get; set; } = false;

		bool selected;
		public bool Selected
		{
			get { return selected; }
			set { selected = value; OnPropertyChanged(nameof(Selected)); }
		}

		bool regexSearch;
		public bool RegexSearch
		{
			get { return regexSearch; }
			set { regexSearch = value; OnPropertyChanged(nameof(RegexSearch)); }
		}

		bool findAllPrases;
		public bool FindAllPhrases
		{
			get { return findAllPrases; }
			set { findAllPrases = value; OnPropertyChanged(nameof(FindAllPhrases)); }
		}

		bool showOnlyHits;
		public bool ShowOnlyHits
		{
			get { return showOnlyHits; }
			set { showOnlyHits = value; OnPropertyChanged(nameof(ShowOnlyHits)); }
		}

		int surroundingLines;
		public int SurroundingLines
		{
			get { return surroundingLines; }
			set { surroundingLines = value; OnPropertyChanged(nameof(SurroundingLines)); }
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

		ObservableCollection<string> errors = new ObservableCollection<string>();
		public ObservableCollection<string> Errors
		{
			get { return errors; }
			set { errors = value; OnPropertyChanged(nameof(Errors)); }
		}

		ObservableCollection<string> filesIgnored = new ObservableCollection<string>();
		public ObservableCollection<string> FilesIgnored
		{
			get { return filesIgnored; }
			set { filesIgnored = value; OnPropertyChanged(nameof(FilesIgnored)); }
		}

		Dictionary<string, int> phraseSums = new Dictionary<string, int>();
		[IgnoreDataMember]
		public Dictionary<string, int> PhraseSums
		{
			get { return phraseSums; }
			set { phraseSums = value; OnPropertyChanged(nameof(PhraseSums)); }
		}

		string statusText;
		[IgnoreDataMember]
		public string StatusText
		{
			get { return statusText; }
			set { statusText = value; OnPropertyChanged(nameof(StatusText)); }
		}

		string fileCountStatus;
		[IgnoreDataMember]
		public string FileCountStatus
		{
			get { return fileCountStatus; }
			set { fileCountStatus = value; OnPropertyChanged(nameof(FileCountStatus)); }
		}

		string errorCountStatus;
		[IgnoreDataMember]
		public string ErrorCountStatus
		{
			get { return errorCountStatus; }
			set { errorCountStatus = value; OnPropertyChanged(nameof(ErrorCountStatus)); OnPropertyChanged(nameof(AnyErrors)); }
		}

		string ignoredFilesCountStatus;
		[IgnoreDataMember]
		public string IgnoredFilesCountStatus
		{
			get { return ignoredFilesCountStatus; }
			set { ignoredFilesCountStatus = value; OnPropertyChanged(nameof(IgnoredFilesCountStatus)); OnPropertyChanged(nameof(AnyIgnoredFiles)); }
		}

		public bool AnyErrors
		{
			get { return Errors.Count > 0; }
		}

		public bool AnyIgnoredFiles
		{
			get { return FilesIgnored.Count > 0; }
		}

		int progress;
		[IgnoreDataMember]
		public int Progress
		{
			get { return progress; }
			set { progress = value; OnPropertyChanged(nameof(Progress)); }
		}

		bool caseSensitive;
		public bool CaseSensitive
		{
			get { return caseSensitive; }
			set { caseSensitive = value; OnPropertyChanged(nameof(CaseSensitive)); }
		}

		internal List<string> StoredSearchPhrases
		{
			get
			{
				List<string> l = new List<string>();

				if (FilesWithHits.Count > 0)
				{
					foreach (KeyValuePair<string, PhraseHit> kvp in FilesWithHits[0].PhraseHits)
					{
						l.Add(kvp.Key);
					}
				}

				return l;
			}
		}

		public string PhraseColumnSetup
		{
			get
			{
				StringBuilder checksum = new StringBuilder();

				foreach (string s in StoredSearchPhrases)
				{
					checksum.Append($"[(<{s}>)];");
				}

				return checksum.ToString();
			}
		}

		public int SearchedFileCount { get; set; }

		public int IgnoredFileCount { get; set; }

		#endregion

		#region Methods

		internal void StartSearch(MainWindow window)
		{
			SearchInProgress = true;

			if (!Renamed)
			{
				foreach (TextAttribute a in SearchPhrases)
				{
					if (a.Used)
					{
						Name = a.Text;
						break;
					}
				}
			}

			mainWindow = window;

			FilesWithHits.Clear();
			Errors.Clear();
			FilesIgnored.Clear();
			StatusText = "";
			FileCountStatus = "";
			ErrorCountStatus = "";

			backgroundSearch = new BackgroundSearch(this);
		}

		internal void CancelSearch()
		{
			backgroundSearch.CancelSearch();
		}

		private void SearchProgressUpdate(List<FileHit> searchResults, List<string> searchErrors, List<string> searchIgnores, string statusText, int percentageComplete, int filesSearched)
		{
			StatusText = statusText;
			Progress = percentageComplete;
			SearchedFileCount = filesSearched;
			IgnoredFileCount = searchIgnores.Count;

			for (int i = FilesWithHits.Count; i < searchResults.Count; i++)
			{
				FilesWithHits.Add(searchResults[i]);
			}

			for (int i = Errors.Count; i < searchErrors.Count; i++)
			{
				Errors.Add(searchErrors[i]);
			}

			for (int i = FilesIgnored.Count; i < searchIgnores.Count; i++)
			{
				FilesIgnored.Add(searchIgnores[i]);
			}

			if (mainWindow.ActiveSearch == this)
			{
				mainWindow.UpdateStats();
			}
		}

		#endregion

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		#endregion

	}
}
