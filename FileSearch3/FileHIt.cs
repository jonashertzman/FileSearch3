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

		bool visible = true;
		public bool Visible
		{
			get { return visible; }
			set { visible = value; OnPropertyChanged(nameof(Visible)); }
		}

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

		internal bool AnyPhraseHit(bool caseSensitive)
		{
			if (PhraseHits.Count == 0)
				return true;

			if (caseSensitive)
			{
				foreach (KeyValuePair<string, PhraseHit> kvp in PhraseHits)
				{
					if (kvp.Value.CaseSensitiveCount > 0)
					{
						return true;
					}
				}
			}
			else
			{
				foreach (KeyValuePair<string, PhraseHit> kvp in PhraseHits)
				{
					if (kvp.Value.Count > 0)
					{
						return true;
					}
				}
			}
			return false;
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
