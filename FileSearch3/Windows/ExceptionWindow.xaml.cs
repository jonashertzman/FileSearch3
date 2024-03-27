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

	string errorType;
	public string ErrorType
	{
		get { return errorType; }
		set { errorType = value; OnPropertyChanged(ErrorType); }
	}

	string errorMessage;
	public string ErrorMessage
	{
		get { return errorMessage; }
		set { errorMessage = value; OnPropertyChanged(ErrorMessage); }
	}

	string callStack;
	public string CallStack
	{
		get { return callStack; }
		set { callStack = value; OnPropertyChanged(CallStack); }
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
