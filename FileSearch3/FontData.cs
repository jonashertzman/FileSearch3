using System.Windows.Media;

namespace FileSearch;

class FontData(GlyphTypeface glyphTypeface, double topDistance, double bottomDistance, bool heightsCalculated)
{

	public GlyphTypeface GlyphTypeface { get; internal set; } = glyphTypeface;
	public double TopDistance { get; internal set; } = topDistance;
	public double BottomDistance { get; internal set; } = bottomDistance;
	public bool HeightsCalculated { get; internal set; } = heightsCalculated;

}
