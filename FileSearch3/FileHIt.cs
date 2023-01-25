using System.ComponentModel;
using System.Runtime.Serialization;

namespace FileSearch;

public class FileHit : INotifyPropertyChanged
{

	#region Constructors

	public FileHit()
	{
	}

	public FileHit(string path, List<TextAttribute> searchPhrases, WIN32_FIND_DATA findData, bool isFolder = false)
	{
		this.Path = path;
		IsFolder = isFolder;

		if (!isFolder)
		{
			Size = (long)Combine(findData.nFileSizeHigh, findData.nFileSizeLow);
		}

		Date = DateTime.FromFileTime((long)Combine(findData.ftLastWriteTime.dwHighDateTime, findData.ftLastWriteTime.dwLowDateTime));

		foreach (TextAttribute t in searchPhrases)
		{
			PhraseHits.Add(t.Text, new PhraseHit());
		}
	}

	#endregion

	#region Properties

	public string Path { get; set; }

	public DateTime Date { get; set; }

	public long Size { get; set; }

	internal bool Selected { get; set; }

	public bool IsFolder { get; set; }

	public Dictionary<string, PhraseHit> PhraseHits { get; set; } = new Dictionary<string, PhraseHit>();

	[IgnoreDataMember]
	public List<PhraseHit> PhraseHitsList
	{
		get
		{
			List<PhraseHit> l = new List<PhraseHit>();
			foreach (KeyValuePair<string, PhraseHit> kvp in PhraseHits)
			{
				l.Add(kvp.Value);
			}
			return l;
		}
	}

	bool visible = true;
	[IgnoreDataMember]
	public bool Visible
	{
		get { return visible; }
		set { visible = value; OnPropertyChanged(nameof(Visible)); }
	}

	#endregion

	#region Methods

	private ulong Combine(uint highValue, uint lowValue)
	{
		return (ulong)highValue << 32 | lowValue;
	}

	internal void AddPhraseHit(string phrase, bool caseSensitiveHit)
	{
		PhraseHits[phrase].Count++;
		if (caseSensitiveHit)
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

	internal bool AllPhrasesHit(bool caseSensitive)
	{
		if (PhraseHits.Count == 0)
			return true;

		if (caseSensitive)
		{
			foreach (KeyValuePair<string, PhraseHit> kvp in PhraseHits)
			{
				if (kvp.Value.CaseSensitiveCount == 0)
				{
					return false;
				}
			}
		}
		else
		{
			foreach (KeyValuePair<string, PhraseHit> kvp in PhraseHits)
			{
				if (kvp.Value.Count == 0)
				{
					return false;
				}
			}
		}
		return true;
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
