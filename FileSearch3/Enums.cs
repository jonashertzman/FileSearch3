using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSearch
{
	public enum TextState
	{
		FullMatch,
		PartialMatch,
		Deleted,
		New,
		Filler,
		Ignored
	}

	public enum NewlineMode
	{
		Windows,
		Unix,
		Mac
	}
}
