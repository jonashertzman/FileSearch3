using System.ComponentModel;
using System.Windows;

namespace FileSearch;

public partial class ExceptionWindow : Window, INotifyPropertyChanged
{

	#region Constructor

	public ExceptionWindow()
	{
		InitializeComponent();
		DataContext = this;
	}

	#endregion

	#region Properties

	string exceptionType;
	public string ExceptionType
	{
		get { return exceptionType; }
		set { exceptionType = value; OnPropertyChanged(ExceptionType); }
	}

	string exceptionMessage;
	public string ExceptionMessage
	{
		get { return exceptionMessage; }
		set { exceptionMessage = value; OnPropertyChanged(ExceptionMessage); }
	}

	string source;
	public string Source
	{
		get { return source; }
		set { source = value; OnPropertyChanged(Source); }
	}

	string stackTrace;
	public string StackTrace
	{
		get { return stackTrace; }
		set { stackTrace = value; OnPropertyChanged(StackTrace); }
	}

	#endregion

	#region Events

	private void Button_Click(object sender, RoutedEventArgs e)
	{
		this.Close();
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
