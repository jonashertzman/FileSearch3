using System.ComponentModel;

namespace FileSearch
{
	public class SearchPhrase : INotifyPropertyChanged
	{
		public SearchPhrase()
		{

		}

		string phrase;
		public string Phrase
		{
			get { return phrase; }
			set { phrase = value; OnPropertyChanged("Phrase"); }
		}

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(name));
			}
		}

		#endregion

	}
}
