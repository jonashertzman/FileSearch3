using System.Windows;

namespace FileSearch
{
	/// <summary>
	/// Interaction logic for RenameTabWindow.xaml
	/// </summary>
	public partial class RenameTabWindow : Window
	{

		#region Constructor

		public RenameTabWindow()
		{
			InitializeComponent();
		}

		#endregion

		#region Properties

		public string TabName
		{
			get { return TextBoxName.Text; }
			set { TextBoxName.Text = value; }
		}

		#endregion

		#region Events

		private void ButtonOk_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			TextBoxName.Focus();
			TextBoxName.SelectAll();
		}

		#endregion

	}
}
