using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace FileSearch
{
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
				TextSegments.Add(new TextSegment(value, Type));
			}
		}

		public ObservableCollection<TextSegment> TextSegments { get; set; } = new ObservableCollection<TextSegment>();

		public bool IsWhitespaceLine { get; set; }

		public List<char> Characters
		{
			get
			{
				List<char> list = new List<char>();

				foreach (char c in text.ToCharArray())
				{
					list.Add(c);
				}
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
				type = value;
				TextSegments.Clear();
				TextSegments.Add(new TextSegment(Text, value));
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
						return AppSettings.HitForeground;
					case TextState.SurroundSpacing:
						return AppSettings.HeaderForeground;

					default:
						return AppSettings.NormalForeground;
				}
			}
		}

		public GlyphRun RenderedText { get; private set; }
		public object CurrentFile { get; internal set; }

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
			TextSegments.Clear();

			int start = 0;
			for (int i = 1; i < Text.Length; i++)
			{
				if (hitCharacters[start] == hitCharacters[i])
					continue;

				TextSegments.Add(new TextSegment(Text.Substring(start, i - start), hitCharacters[start] ? TextState.Hit : TextState.Miss));
				start = i;
			}
			TextSegments.Add(new TextSegment(Text.Substring(start, Text.Length - start), hitCharacters[start] ? TextState.Hit : TextState.Miss));
		}

		#endregion

	}
}
