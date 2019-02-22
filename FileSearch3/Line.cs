using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
			return $"{LineIndex}  {Text}  {MatchingLineIndex}";
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

		public int? LineIndex { get; set; }

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
						return AppSettings.HeaderTextBackground;
					case TextState.Hit:
						return AppSettings.HitTextBackground;

					default:
						return AppSettings.NormalTextBackground;
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
						return AppSettings.HeaderTextForeground;
					case TextState.Hit:
						return AppSettings.HitTextForeground;

					default:
						return AppSettings.NormalTextForeground;
				}
			}
		}

		public int? MatchingLineIndex { get; set; }

		public GlyphRun RenderedText { get; private set; }
		private double renderedTextWidth;

		private int? renderedLineIndex;
		private Typeface renderedTypeface;
		private double renderedFontSize;
		private double renderedDpiScale;

		public GlyphRun GetRenderedLineIndexText(Typeface typeface, double fontSize, double dpiScale, out double runWidth)
		{
			if (renderedLineIndex != LineIndex || !typeface.Equals(renderedTypeface) || fontSize != renderedFontSize || dpiScale != renderedDpiScale)
			{
				RenderedText = TextUtils.CreateGlyphRun(LineIndex == -1 ? "+" : LineIndex.ToString(), typeface, fontSize, dpiScale, out renderedTextWidth);

				renderedLineIndex = LineIndex;
				renderedTypeface = typeface;
				renderedFontSize = fontSize;
				renderedDpiScale = dpiScale;
			}

			runWidth = renderedTextWidth;
			return RenderedText;
		}

		#endregion

	}
}
