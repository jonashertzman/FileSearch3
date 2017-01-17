using System.Collections.ObjectModel;
using System.ComponentModel;

namespace FileSearch
{
	public class SearchInstance : INotifyPropertyChanged
	{

		#region Constructor

		public SearchInstance()
		{
			Name = "A";
		}

		#endregion

		public override string ToString()
		{
			return name;
		}

		#region Properties

		string name;
		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				name = value;
				OnPropertyChanged("Name");
			}
		}

		bool isSelected;
		public bool IsSelected
		{
			get
			{
				return isSelected;
			}
			set
			{
				isSelected = value;
				OnPropertyChanged("IsSelected");
			}
		}

		ObservableCollection<SearchAttribute> searchPhrases = new ObservableCollection<SearchAttribute>();
		public ObservableCollection<SearchAttribute> SearchPhrases
		{
			get { return searchPhrases; }
			set { searchPhrases = value; OnPropertyChanged("SearchPhrases"); }
		}

		ObservableCollection<SearchAttribute> searchDirectories = new ObservableCollection<SearchAttribute>();
		public ObservableCollection<SearchAttribute> SearchDirectories
		{
			get { return searchDirectories; }
			set { searchDirectories = value; OnPropertyChanged("SearchDirectories"); }
		}

		ObservableCollection<SearchAttribute> searchFiles = new ObservableCollection<SearchAttribute>();
		public ObservableCollection<SearchAttribute> SearchFiles
		{
			get { return searchFiles; }
			set { searchFiles = value; OnPropertyChanged("SearchFiles"); }
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
}
