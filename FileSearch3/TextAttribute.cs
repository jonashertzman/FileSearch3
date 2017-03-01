using System.ComponentModel;

namespace FileSearch
{
	public class TextAttribute : INotifyPropertyChanged
	{
		public TextAttribute()
		{

		}

		public TextAttribute(string v)
		{
			this.text = v;
		}

		string text;
		public string Text
		{
			get { return text; }
			set { text = value; uppercaseText = value.ToUpper(); OnPropertyChanged("Text"); }
		}

		string uppercaseText;
		internal string UppercaseText
		{
			get { return uppercaseText; }
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
