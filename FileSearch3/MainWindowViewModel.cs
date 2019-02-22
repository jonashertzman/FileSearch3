using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Threading;

namespace FileSearch
{
	public class MainWindowViewModel : INotifyPropertyChanged
	{

		#region Members

		DispatcherTimer timer = new DispatcherTimer();

		#endregion

		#region Constructor

		public MainWindowViewModel()
		{
			timer.Interval = new TimeSpan(300000);
			timer.Tick += Timer_Tick;
		}

		#endregion

		#region Properties

		private SearchInstance activeSearchInstance;
		public SearchInstance ActiveSearchInstance
		{
			get { return activeSearchInstance; }
			set { activeSearchInstance = value; OnPropertyChanged(nameof(ActiveSearchInstance)); }
		}

		public ObservableCollection<SearchInstance> SearchInstances
		{
			get { return AppSettings.SearchInstances; }
			set { AppSettings.SearchInstances = value; OnPropertyChanged(nameof(SearchInstances)); }
		}

		ObservableCollection<Line> previewLines = new ObservableCollection<Line>();
		public ObservableCollection<Line> PreviewLines
		{
			get { return previewLines; }
			set { previewLines = value; OnPropertyChangedRepaint(nameof(PreviewLines)); }
		}

		FileEncoding fileEncoding = null;
		public FileEncoding FileEncoding
		{
			get { return fileEncoding; }
			set { fileEncoding = value; OnPropertyChanged(nameof(FileEncoding)); OnPropertyChanged(nameof(FileDescription)); }
		}

		public string FileDescription
		{
			get { return FileEncoding?.ToString(); }
		}

		bool fileDirty = false;
		public bool FileDirty
		{
			get { return fileDirty || fileEdited; }
			set { fileDirty = value; OnPropertyChanged(nameof(FileDirty)); }
		}

		bool fileEdited = false;
		public bool FileEdited
		{
			get { return fileEdited; }
			set { fileEdited = value; OnPropertyChanged(nameof(FileEdited)); OnPropertyChanged(nameof(FileDirty)); }
		}

		bool editMode = false;
		public bool EditMode
		{
			get { return editMode; }
			set { editMode = value; OnPropertyChanged(nameof(EditMode)); }
		}

		public ObservableCollection<TextAttribute> IgnoredFolders
		{
			get { return AppSettings.IgnoredFolders; }
			set { AppSettings.IgnoredFolders = value; OnPropertyChangedRepaint(nameof(IgnoredFolders)); }
		}

		public ObservableCollection<TextAttribute> IgnoredFiles
		{
			get { return AppSettings.IgnoredFiles; }
			set { AppSettings.IgnoredFiles = value; OnPropertyChangedRepaint(nameof(IgnoredFiles)); }
		}

		int currentMatch = -1;
		public int CurrentMatch
		{
			get { return currentMatch; }
			set { currentMatch = value; OnPropertyChangedRepaint(nameof(CurrentMatch)); }
		}

		int currentMatchLength;
		public int CurrentMatchLength
		{
			get { return currentMatchLength; }
			set { currentMatchLength = value; OnPropertyChangedRepaint(nameof(currentMatchLength)); }
		}

		public double NameColumnWidth
		{
			get { return AppSettings.NameColumnWidth; }
			set { AppSettings.NameColumnWidth = value; OnPropertyChangedSlowRepaint(nameof(NameColumnWidth)); }
		}

		public double SizeColumnWidth
		{
			get { return AppSettings.SizeColumnWidth; }
			set { AppSettings.SizeColumnWidth = value; OnPropertyChangedSlowRepaint(nameof(SizeColumnWidth)); }
		}

		public double DateColumnWidth
		{
			get { return AppSettings.DateColumnWidth; }
			set { AppSettings.DateColumnWidth = value; OnPropertyChangedSlowRepaint(nameof(DateColumnWidth)); }
		}

		public SolidColorBrush FullMatchForeground
		{
			get { return AppSettings.NormalTextForeground; }
			set { AppSettings.NormalTextForeground = value; OnPropertyChangedRepaint(nameof(FullMatchForeground)); }
		}

		public SolidColorBrush FullMatchBackground
		{
			get { return AppSettings.NormalTextBackground; }
			set { AppSettings.NormalTextBackground = value; OnPropertyChangedRepaint(nameof(FullMatchBackground)); }
		}

		public SolidColorBrush PartialMatchForeground
		{
			get { return AppSettings.HitTextForeground; }
			set { AppSettings.HitTextForeground = value; OnPropertyChangedRepaint(nameof(PartialMatchForeground)); }
		}

		public SolidColorBrush PartialMatchBackground
		{
			get { return AppSettings.HitTextBackground; }
			set { AppSettings.HitTextBackground = value; OnPropertyChangedRepaint(nameof(PartialMatchBackground)); }
		}

		public SolidColorBrush IgnoredForeground
		{
			get { return AppSettings.HeaderTextForeground; }
			set { AppSettings.HeaderTextForeground = value; OnPropertyChangedRepaint(nameof(IgnoredForeground)); }
		}

		public SolidColorBrush IgnoredBackground
		{
			get { return AppSettings.HeaderTextBackground; }
			set { AppSettings.HeaderTextBackground = value; OnPropertyChangedRepaint(nameof(IgnoredBackground)); }
		}

		public FontFamily Font
		{
			get { return AppSettings.Font; }
			set { AppSettings.Font = value; OnPropertyChangedRepaint(nameof(Font)); }
		}

		public int FontSize
		{
			get { return AppSettings.FontSize; }
			set { AppSettings.FontSize = value; OnPropertyChangedRepaint(nameof(FontSize)); }
		}

		public int TabSize
		{
			get { return AppSettings.TabSize; }
			set { AppSettings.TabSize = Math.Max(1, value); OnPropertyChangedRepaint(nameof(TabSize)); }
		}

		int updateTrigger;
		public int UpdateTrigger
		{
			get { return updateTrigger; }
			set { updateTrigger = value; OnPropertyChanged(nameof(UpdateTrigger)); }
		}

		#endregion

		#region Methods


		#endregion

		#region Events

		private void Timer_Tick(object sender, EventArgs e)
		{
			timer.Stop();
			UpdateTrigger++;
		}

		#endregion

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChangedSlowRepaint(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

			if (!timer.IsEnabled)
			{
				timer.Start();
			}
		}

		public void OnPropertyChangedRepaint(string name)
		{
			UpdateTrigger++;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		public void OnPropertyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		#endregion

	}
}
