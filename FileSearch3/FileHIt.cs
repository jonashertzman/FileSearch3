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

		public FileHit(string path)
		{
			this.Path = path;
		}

		#endregion

		#region Properties

		public string Path { get; set; }

		public bool Selected { get; set; }

		public Dictionary<string, PhraseHit> PhraseHits { get; set; } = new Dictionary<string, PhraseHit>();

		internal int FoundPhrasesCount
		{
			get
			{
				return PhraseHits.Count;
			}
		}

		internal int FoundPhrasesCaseSensitiveCount
		{
			get
			{
				int i = 0;

				foreach (KeyValuePair<string, PhraseHit> entry in PhraseHits)
				{
					if (entry.Value.CaseSensitiveCount > 0)
					{
						i++;
					}
				}
				return i;
			}
		}

		#endregion

		#region Methods

		internal void AddPhraseHit(string phrase, bool caseSensieiveHit)
		{
			if (PhraseHits.ContainsKey(phrase))
			{
				PhraseHits[phrase].Count++;
				if (caseSensieiveHit)
				{
					PhraseHits[phrase].CaseSensitiveCount++;
				}
			}
			else
			{
				PhraseHits.Add(phrase, new PhraseHit(caseSensieiveHit));
			}
		}

		internal int GetNumberOfHits(string searchPhrase)
		{
			if (PhraseHits.ContainsKey(searchPhrase))
			{
				return PhraseHits[searchPhrase].Count;
			}
			return 0;
		}

		internal int GetNumberOfCaseSensitiveHits(string searchPhrase)
		{
			if (PhraseHits.ContainsKey(searchPhrase))
			{
				return PhraseHits[searchPhrase].CaseSensitiveCount;
			}
			return 0;
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
