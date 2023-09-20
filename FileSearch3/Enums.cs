namespace FileSearch;

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
	Mac,
	Mixed,
}

public enum LogWindowType
{
	Errors,
	IgnoredFiles
}

public enum Themes
{
	Light,
	Dark,
}
