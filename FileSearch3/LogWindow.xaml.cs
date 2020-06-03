using System.Windows;

namespace FileSearch
{
	public partial class LogWindow : Window
	{

		#region Constructor

		public LogWindow()
		{
			InitializeComponent();

			Utils.HideMinimizeAndMaximizeButtons(this);
		}

		#endregion

		#region Properties

		public LogWindowType Type { get; set; }

		#endregion

		#region Events

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			SearchErrorsList.Visibility = Type == LogWindowType.Errors ? Visibility.Visible : Visibility.Collapsed;
			IgnoredFilesList.Visibility = Type == LogWindowType.IgnoredFiles ? Visibility.Visible : Visibility.Collapsed;

			if (Type == LogWindowType.IgnoredFiles)
			{
				Title = "Ignored Files";
			}
		}

		#endregion

	}
}
