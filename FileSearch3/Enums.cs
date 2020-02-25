namespace FileSearch
{

	public enum TextState
	{
		Miss,
		Hit,
		Header,
		Filler,
		Surround,
		SurroundSpacing
	}

	public enum NewlineMode
	{
		Windows,
		Unix,
		Mac
	}

	public enum LogWindowType
	{
		Errors,
		IgnoredFiles
	}

}
