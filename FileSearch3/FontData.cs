using System.Windows.Media;

namespace FileSearch;

class FontData
{

	public FontData(GlyphTypeface glyphTypeface, double topDistance, double bottomDistance, bool heightsCalculated)
	{
		GlyphTypeface = glyphTypeface;
		TopDistance = topDistance;
		BottomDistance = bottomDistance;
		HeightsCalculated = heightsCalculated;
	}

	public GlyphTypeface GlyphTypeface { get; internal set; }
	public double TopDistance { get; internal set; }
	public double BottomDistance { get; internal set; }
	public bool HeightsCalculated { get; internal set; }

}
