namespace FileSearch
{
	public enum TextState
	{
		Miss,
		Hit,
		Header,
		Filler,
		Surround
	}

	public enum NewlineMode
	{
		Windows,
		Unix,
		Mac
	}
}
