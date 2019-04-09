using System.Collections.Generic;
using System.ComponentModel;

namespace FileSearch
{
	public class FileHit : INotifyPropertyChanged
	{

		#region Constructors

		public FileHit()
		{
		}

		public FileHit(string path, List<TextAttribute> searchPhrases)
		{
			this.Path = path;

			foreach (TextAttribute t in searchPhrases)
			{
				PhraseHits.Add(t.Text, new PhraseHit());
			}
		}

		#endregion

		#region Properties

		public string Path { get; set; }

		internal bool Selected { get; set; }

		public Dictionary<string, PhraseHit> PhraseHits { get; set; } = new Dictionary<string, PhraseHit>();

		#endregion

		#region Methods

		internal void AddPhraseHit(string phrase, bool caseSensieiveHit)
		{
			PhraseHits[phrase].Count++;
			if (caseSensieiveHit)
			{
				PhraseHits[phrase].CaseSensitiveCount++;
			}
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
