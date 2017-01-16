using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace FileSearch
{

	public class AppSettings : INotifyPropertyChanged
	{

		#region Members

		ObservableCollection<SearchInstance> searchInstances = new ObservableCollection<SearchInstance>();

		#endregion

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
