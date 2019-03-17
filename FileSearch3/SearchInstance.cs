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

		internal delegate void SearchProgressUpdateDelegate(List<FileHit> SearchResults, String statusText, int percentageComplete);
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

		public bool SearchInProgress
		{
			get {return backgroundSearch!= null && backgroundSearch.SearchInProgress; }
			set { OnPropertyChanged(nameof(SearchInProgress)); }
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

		#endregion

		#region Methods

		internal void StartSearch(MainWindow window)
		{
			SearchInProgress = true;

			mainWindow = window;

			FilesWithHits.Clear();
			//LogedItems.Clear();

			SearchedFilesCount = 0;

			backgroundSearch = new BackgroundSearch(this);
		}

		internal void CancelSearch()
		{
			backgroundSearch.CancelSaerch();
		}

		private void SearchProgressUpdate(List<FileHit> SearchResults, string statusText, int percentageComplete)
		{
			StatusText = statusText;
			Progress = percentageComplete;

			for (int i = FilesWithHits.Count; i < SearchResults.Count; i++)
			{
				FilesWithHits.Add(SearchResults[i]);
			}

			FileCountStatus = $"{FilesWithHits.Count} files found in {SearchedFilesCount} searched";
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
