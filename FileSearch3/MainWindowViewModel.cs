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

		int maxVerialcalScroll;
		public int MaxVerialcalScroll
		{
			get { return maxVerialcalScroll; }
			set { maxVerialcalScroll = value; OnPropertyChanged(nameof(MaxVerialcalScroll)); OnPropertyChanged(nameof(MaxVerialcalScroll)); }
		}

		int visibleLines;
		public int VisibleLines
		{
			get { return visibleLines; }
			set { visibleLines = value; OnPropertyChanged(nameof(VisibleLines)); OnPropertyChanged(nameof(MaxVerialcalScroll)); }
		}

		int currentDiff = -1;
		public int CurrentDiff
		{
			get { return currentDiff; }
			set { currentDiff = value; OnPropertyChangedRepaint(nameof(CurrentDiff)); }
		}

		int currentDiffLength;
		public int CurrentDiffLength
		{
			get { return currentDiffLength; }
			set { currentDiffLength = value; OnPropertyChangedRepaint(nameof(currentDiffLength)); }
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
			get { return AppSettings.FullMatchForeground; }
			set { AppSettings.FullMatchForeground = value; OnPropertyChangedRepaint(nameof(FullMatchForeground)); }
		}

		public SolidColorBrush FullMatchBackground
		{
			get { return AppSettings.FullMatchBackground; }
			set { AppSettings.FullMatchBackground = value; OnPropertyChangedRepaint(nameof(FullMatchBackground)); }
		}

		public SolidColorBrush PartialMatchForeground
		{
			get { return AppSettings.PartialMatchForeground; }
			set { AppSettings.PartialMatchForeground = value; OnPropertyChangedRepaint(nameof(PartialMatchForeground)); }
		}

		public SolidColorBrush PartialMatchBackground
		{
			get { return AppSettings.PartialMatchBackground; }
			set { AppSettings.PartialMatchBackground = value; OnPropertyChangedRepaint(nameof(PartialMatchBackground)); }
		}

		public SolidColorBrush DeletedForeground
		{
			get { return AppSettings.DeletedForeground; }
			set { AppSettings.DeletedForeground = value; OnPropertyChangedRepaint(nameof(DeletedForeground)); }
		}

		public SolidColorBrush DeletedBackground
		{
			get { return AppSettings.DeletedBackground; }
			set { AppSettings.DeletedBackground = value; OnPropertyChangedRepaint(nameof(DeletedBackground)); }
		}

		public SolidColorBrush NewForeground
		{
			get { return AppSettings.NewForeground; }
			set { AppSettings.NewForeground = value; OnPropertyChangedRepaint(nameof(NewForeground)); }
		}

		public SolidColorBrush NewBackground
		{
			get { return AppSettings.NewBackground; }
			set { AppSettings.NewBackground = value; OnPropertyChangedRepaint(nameof(NewBackground)); }
		}

		public SolidColorBrush IgnoredForeground
		{
			get { return AppSettings.IgnoredForeground; }
			set { AppSettings.IgnoredForeground = value; OnPropertyChangedRepaint(nameof(IgnoredForeground)); }
		}

		public SolidColorBrush IgnoredBackground
		{
			get { return AppSettings.IgnoredBackground; }
			set { AppSettings.IgnoredBackground = value; OnPropertyChangedRepaint(nameof(IgnoredBackground)); }
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
