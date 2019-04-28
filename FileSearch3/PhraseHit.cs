using System;

namespace FileSearch
{
	public class PhraseHit
	{

		#region Constructors

		public PhraseHit()
		{

		}

		#endregion

		#region Properties

		public int Count { get; set; } = 0;

		public int CaseSensitiveCount { get; set; } = 0;

		internal int GetCount(bool caseSensitive)
		{
			if (caseSensitive)
			{
				return CaseSensitiveCount;
			}
			return Count;
		}

		#endregion

	}
}
