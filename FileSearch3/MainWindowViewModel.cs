using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace FileSearch
{
	public class MainWindowViewModel : INotifyPropertyChanged
	{

		#region Members

		readonly DispatcherTimer timer = new DispatcherTimer();

		#endregion

		#region Constructor

		public MainWindowViewModel()
		{
			timer.Interval = new TimeSpan(300000);
			timer.Tick += Timer_Tick;
		}

		#endregion

		#region Properties

		public string Title
		{
			get { return "File Search"; }
		}

		public string Version
		{
			get { return "3.0"; }
		}

		public string BuildNumber
		{
			get
			{
				DateTime buildDate = new FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTime;
				return $"{buildDate:yy}{buildDate.DayOfYear:D3}";
			}
		}

		public string FullApplicationName
		{
			get { return $"{Title} {Version}  (Build {BuildNumber})"; }
		}

		bool newBuildAvailable = false;
		public bool NewBuildAvailable
		{
			get { return newBuildAvailable; }
			set { newBuildAvailable = value; OnPropertyChanged(nameof(NewBuildAvailable)); }
		}



		SearchInstance activeSearchInstance;
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

		int currentHit;
		public int CurrentHit
		{
			get { return currentHit; }
			set { currentHit = value; OnPropertyChanged(nameof(CurrentHit)); }
		}

		FileEncoding fileEncoding = null;
		public FileEncoding FileEncoding
		{
			get { return fileEncoding; }
			set { fileEncoding = value; OnPropertyChanged(nameof(FileEncoding)); }
		}

		bool fileEdited = false;
		public bool FileEdited
		{
			get { return fileEdited; }
			set { fileEdited = value; OnPropertyChanged(nameof(FileEdited)); }
		}

		bool editMode = false;
		public bool EditMode
		{
			get { return editMode; }
			set { editMode = value; OnPropertyChanged(nameof(EditMode)); }
		}

		public bool ShowWhiteSpaceCharacters
		{
			get { return AppSettings.ShowWhiteSpaceCharacters; }
			set { AppSettings.ShowWhiteSpaceCharacters = value; OnPropertyChangedRepaint(nameof(ShowWhiteSpaceCharacters)); }
		}

		public ObservableCollection<TextAttribute> IgnoredDirectories
		{
			get { return AppSettings.IgnoredDirectories; }
			set { AppSettings.IgnoredDirectories = value; OnPropertyChangedRepaint(nameof(IgnoredDirectories)); }
		}

		public ObservableCollection<TextAttribute> IgnoredFiles
		{
			get { return AppSettings.IgnoredFiles; }
			set { AppSettings.IgnoredFiles = value; OnPropertyChangedRepaint(nameof(IgnoredFiles)); }
		}

		public GridLength PhraseGridHeight
		{
			get { return new GridLength(AppSettings.PhraseGridHeight); }
			set { AppSettings.PhraseGridHeight = value.Value; OnPropertyChanged(nameof(PhraseGridHeight)); }
		}

		public GridLength DirectoriesGridHeight
		{
			get { return new GridLength(AppSettings.DirectoriesGridHeight); }
			set { AppSettings.DirectoriesGridHeight = value.Value; OnPropertyChanged(nameof(DirectoriesGridHeight)); }
		}

		public GridLength SearchAttributesWidth
		{
			get { return new GridLength(AppSettings.SearchAttributesWidth); }
			set { AppSettings.SearchAttributesWidth = value.Value; OnPropertyChanged(nameof(SearchAttributesWidth)); }
		}

		public GridLength FileListHeight
		{
			get { return new GridLength(AppSettings.FileListHeight); }
			set { AppSettings.FileListHeight = value.Value; OnPropertyChanged(nameof(FileListHeight)); }
		}

		public Brush NormalForeground
		{
			get { return AppSettings.NormalForeground; }
			set { AppSettings.NormalForeground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(NormalForeground)); }
		}

		public Brush NormalBackground
		{
			get { return AppSettings.NormalBackground; }
			set { AppSettings.NormalBackground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(NormalBackground)); }
		}

		public Brush HeaderForeground
		{
			get { return AppSettings.HeaderForeground; }
			set { AppSettings.HeaderForeground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(HeaderForeground)); }
		}

		public Brush HeaderBackground
		{
			get { return AppSettings.HeaderBackground; }
			set { AppSettings.HeaderBackground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(HeaderBackground)); }
		}

		public Brush HitForeground
		{
			get { return AppSettings.HitForeground; }
			set { AppSettings.HitForeground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(HitForeground)); }
		}

		public Brush HitBackground
		{
			get { return AppSettings.HitBackground; }
			set { AppSettings.HitBackground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(HitBackground)); }
		}

		public Brush SelectionBackground
		{
			get { return AppSettings.SelectionBackground; }
			set { AppSettings.SelectionBackground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(SelectionBackground)); }
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
