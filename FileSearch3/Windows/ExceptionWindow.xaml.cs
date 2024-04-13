using System.ComponentModel;
using System.Net.Http;
using System.Text;
using System.Text.Json;
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

	private void CloseButton_Click(object sender, RoutedEventArgs e)
	{
		this.Close();
	}

	private async void ReportButton_Click(object sender, RoutedEventArgs e)
	{
		HttpClient httpClient = new();

		CrashReport cr = new()
		{
			ApplicationName = "FileDiff",
			ClientId = AppSettings.Id,
			BuildNumber = AppSettings.BuildNumber,
			StackTrace = this.StackTrace

		};

		string json = JsonSerializer.Serialize(cr);
		var content = new StringContent(json, Encoding.UTF8, "application/json");

		var response = await httpClient.PostAsync("https://localhost:7133/api/CrashReport", content);

		//this.Close();

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

class CrashReport
{
	public string ApplicationName { get; set; } = "FileDiff";

	public string ClientId { get; set; } = "";

	public string BuildNumber { get; set; }

	public string StackTrace { get; set; }
}