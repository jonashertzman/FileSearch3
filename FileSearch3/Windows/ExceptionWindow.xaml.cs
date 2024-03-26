using System.Windows;

namespace FileSearch;

public partial class ExceptionWindow : Window
{
	public ExceptionWindow()
	{
		InitializeComponent();
	}

	private void Button_Click(object sender, RoutedEventArgs e)
	{
		this.Close();
	}
}
