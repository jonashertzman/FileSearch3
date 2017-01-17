using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace FileSearch
{

	public class AppSettings : INotifyPropertyChanged
	{

		#region Properties

		[IgnoreDataMemberAttribute]
		public SearchInstance ActiveSearchInstance
		{
			get
			{
				foreach (SearchInstance s in searchInstances)
				{
					if (s.IsSelected)
					{
						return s;
					}
				}
				return searchInstances[0];
			}
			set
			{
				OnPropertyChanged("ActiveSearchInstance");
			}
		}

		ObservableCollection<SearchInstance> searchInstances = new ObservableCollection<SearchInstance>();
		public ObservableCollection<SearchInstance> SearchInstances
		{
			get
			{
				return searchInstances;
			}
			set
			{
				searchInstances = value;
				OnPropertyChanged("SearchInstances");
			}
		}

		int windowWidth = 800;
		public int WindowWidth
		{
			get { return windowWidth; }
			set { windowWidth = value; OnPropertyChanged("WindowWidth"); }
		}

		int windowHeight = 600;
		public int WindowHeight
		{
			get { return windowHeight; }
			set { windowHeight = value; OnPropertyChanged("WindowHeight"); }
		}

		int phrasesHeight = 100;
		public int PhrasesHeight
		{
			get { return phrasesHeight; }
			set { phrasesHeight = value; OnPropertyChanged("PhrasesHeight"); }
		}

		int directoriesHeight = 100;
		public int DirectoriesHeight
		{
			get { return directoriesHeight; }
			set { directoriesHeight = value; OnPropertyChanged("DirectoriesHeight"); }
		}

		#endregion

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
