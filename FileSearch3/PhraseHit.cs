namespace FileSearch
{
	public class PhraseHit
	{

		#region Constructors

		public PhraseHit()
		{

		}

		public PhraseHit(bool caseSensitive)
		{
			this.Count = 1;
			this.CaseSensitiveCount = caseSensitive ? 1 : 0;
		}

		#endregion

		#region Properties

		public int Count { get; set; }

		public int CaseSensitiveCount { get; set; }

		#endregion

	}
}
