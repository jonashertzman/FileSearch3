using System.ComponentModel;

namespace FileSearch
{
	public class TextAttribute : INotifyPropertyChanged
	{
		public TextAttribute()
		{

		}

		public TextAttribute(string text)
		{
			this.Text = text;
		}

		public TextAttribute(string text, bool used)
		{
			this.Text = text;
			this.Used = used;
		}

		string text;
		public string Text
		{
			get { return text; }
			set { text = value; UppercaseText = value.ToUpper(); OnPropertyChanged(nameof(Text)); }
		}

		bool used = true;
		public bool Used
		{
			get { return used; }
			set { used = value; OnPropertyChanged(nameof(Used)); }
		}


		internal string UppercaseText
		{
			get; private set;
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
