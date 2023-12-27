using System.Collections.ObjectModel;
using System.Windows.Media;

namespace FileSearch;

public class Line
{

	#region Constructor

	public Line()
	{

	}

	#endregion

	#region Overrides

	public override string ToString()
	{
		return $"{LineNumber}  {Text}  {Type}";
	}

	#endregion

	#region Properties

	private string text = "";
	public string Text
	{
		get { return text; }
		set
		{
			text = value;
			TextSegments.Clear();
			AddTextSegment(value, Type);
		}
	}

	public ObservableCollection<TextSegment> TextSegments { get; set; } = [];

	public bool IsWhitespaceLine { get; set; }

	public List<char> Characters
	{
		get
		{
			List<char> list = [.. text.ToCharArray()];
			return list;
		}
	}

	public int? LineNumber { get; set; }

	private TextState type;
	public TextState Type
	{
		get { return type; }
		set
		{
			if (type != value)
			{
				type = value;
				TextSegments.Clear();
				AddTextSegment(Text, value);
			}
		}
	}

	public SolidColorBrush BackgroundBrush
	{
		get
		{
			switch (type)
			{
				case TextState.Header:
					return AppSettings.HeaderBackground;
				case TextState.Hit:
					return AppSettings.HitBackground;

				default:
					return AppSettings.NormalBackground;
			}
		}
	}

	public SolidColorBrush ForegroundBrush
	{
		get
		{
			switch (type)
			{
				case TextState.Header:
					return AppSettings.HeaderForeground;
				case TextState.Hit:
					return AppSettings.NormalForeground;
				case TextState.SurroundSpacing:
					return AppSettings.HeaderForeground;

				default:
					return AppSettings.NormalForeground;
			}
		}
	}

	public GlyphRun RenderedText { get; private set; }

	public string CurrentFile { get; internal set; }

	private double renderedTextWidth;
	private int? renderedLineIndex;
	private Typeface renderedTypeface;
	private double renderedFontSize;
	private double renderedDpiScale;

	public GlyphRun GetRenderedLineNumberText(Typeface typeface, double fontSize, double dpiScale, out double runWidth)
	{
		if (renderedLineIndex != LineNumber || !typeface.Equals(renderedTypeface) || fontSize != renderedFontSize || dpiScale != renderedDpiScale)
		{
			RenderedText = TextUtils.CreateGlyphRun(LineNumber == -1 ? "+" : LineNumber.ToString(), typeface, fontSize, dpiScale, out renderedTextWidth);

			renderedLineIndex = LineNumber;
			renderedTypeface = typeface;
			renderedFontSize = fontSize;
			renderedDpiScale = dpiScale;
		}

		runWidth = renderedTextWidth;
		return RenderedText;
	}

	#endregion

	#region Methods

	internal void AddHitSegments(bool[] hitCharacters)
	{
		if (hitCharacters.Length == 0)
			return;

		TextSegments.Clear();

		int start = 0;
		for (int i = 1; i < Text.Length; i++)
		{
			if (hitCharacters[start] == hitCharacters[i])
				continue;

			AddTextSegment(Text[start..i], hitCharacters[start] ? TextState.Hit : TextState.Miss);
			start = i;
		}
		AddTextSegment(Text[start..], hitCharacters[start] ? TextState.Hit : TextState.Miss);
	}

	public void AddTextSegment(string text, TextState state)
	{
		int start = 0;
		int segmentLength;

		do
		{
			segmentLength = Math.Min(1000, text.Length - start);
			if (segmentLength > 0 && char.IsHighSurrogate(text[start + segmentLength - 1]))
			{
				segmentLength--;
			}

			TextSegments.Add(new TextSegment(text.Substring(start, segmentLength), state));
			start += segmentLength;
		} while (text.Length > start);
	}

	#endregion

}
