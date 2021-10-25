namespace FileSearch;

class RegexHit
{
	public RegexHit()
	{
	}

	public RegexHit(int start, int length)
	{
		Start = start;
		Length = length;
	}

	public int Start { get; set; }

	public int Length { get; set; }

	public override string ToString()
	{
		return Start + " " + Length;
	}
}
