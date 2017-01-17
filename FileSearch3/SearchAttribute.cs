using System.ComponentModel;

namespace FileSearch
{
	public class SearchAttribute : INotifyPropertyChanged
	{
		public SearchAttribute()
		{

		}

		string text;
		public string Text
		{
			get { return text; }
			set { text = value; OnPropertyChanged("Text"); }
		}

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		#endregion

	}
}
