using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace FileSearch
{
	public class SearchInstance : INotifyPropertyChanged
	{

		#region Members

		internal MainWindow mainWindow;

		BackgroundSearch backgroundSearch;

		internal delegate void SearchProgressUpdateDelegate(List<FileHit> SearchResults, String statusText, int percentageComplete, int filesSearched);
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

		public int FilesSearched { get; set; }

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
			FilesSearched = filesSearched;

			for (int i = FilesWithHits.Count; i < SearchResults.Count; i++)
			{
				FilesWithHits.Add(SearchResults[i]);
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
