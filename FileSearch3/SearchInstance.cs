using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace FileSearch
{
	public class SearchInstance : INotifyPropertyChanged
	{

		#region Members

		internal MainWindow mainWindow;

		BackgroundSearch backgroundSearch;

		internal delegate void SearchProgressUpdateDelegate(List<FileHit> SearchResults, String statusText, int percentageComplete, int filesSearched);
		internal SearchProgressUpdateDelegate searchProgressUpdateDelegate;

		int caseSensitiveFileCount = 0;

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

		Dictionary<string, PhraseHit> phraseSums = new Dictionary<string, PhraseHit>();
		public Dictionary<string, PhraseHit> PhraseSums
		{
			get { return phraseSums; }
			set { phraseSums = value; OnPropertyChanged(nameof(PhraseSums)); }
		}

		string statusText;
		public string StatusText
		{
			get { return statusText; }
			set { statusText = value; OnPropertyChanged(nameof(StatusText)); }
		}

		public string CaseSensitiveFileCountStatus { get; set; }

		string fileCountStatus;
		public string FileCountStatus
		{
			get { return CaseSensitive ? CaseSensitiveFileCountStatus : fileCountStatus; }
			set { fileCountStatus = value; OnPropertyChanged(nameof(FileCountStatus)); }
		}

		int progress;
		public int Progress
		{
			get { return progress; }
			set { progress = value; OnPropertyChanged(nameof(Progress)); }
		}

		bool caseSensitive;
		public bool CaseSensitive
		{
			get { return caseSensitive; }
			set { caseSensitive = value; OnPropertyChanged(nameof(CaseSensitive)); OnPropertyChanged(nameof(FileCountStatus)); }
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
			//LogedItems.Clear();
			StatusText = "";
			FileCountStatus = "";
			caseSensitiveFileCount = 0;

			backgroundSearch = new BackgroundSearch(this);
		}

		internal void CancelSearch()
		{
			backgroundSearch.CancelSaerch();
		}

		private void SearchProgressUpdate(List<FileHit> SearchResults, string statusText, int percentageComplete, int filesSearched)
		{
			StatusText = statusText;
			Progress = percentageComplete;

			for (int i = FilesWithHits.Count; i < SearchResults.Count; i++)
			{
				FileHit fileHit = SearchResults[i];

				fileHit.Visible = fileHit.AnyPhraseHit(CaseSensitive);

				if (fileHit.AnyPhraseHit(true))
				{
					caseSensitiveFileCount++;
				}

				FilesWithHits.Add(fileHit);

				foreach (KeyValuePair<string, PhraseHit> phraseHit in SearchResults[i].PhraseHits)
				{
					PhraseSums[phraseHit.Key].Count += phraseHit.Value.Count;
					PhraseSums[phraseHit.Key].CaseSensitiveCount += phraseHit.Value.CaseSensitiveCount;
				}
			}

			CaseSensitiveFileCountStatus = filesSearched == 0 ? $"{FilesWithHits.Count} files found" : $"{ caseSensitiveFileCount } files found in {filesSearched} searched";
			FileCountStatus = filesSearched == 0 ? $"{FilesWithHits.Count} files found" : $"{ FilesWithHits.Count} files found in {filesSearched} searched";

			if (mainWindow.ActiveSearch == this)
			{
				mainWindow.AddPhraseColumns();
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
