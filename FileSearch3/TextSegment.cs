using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FileSearch
{
	public class TextSegment
	{

		#region Constructor

		public TextSegment()
		{

		}

		public TextSegment(string text, TextState textState)
		{
			this.Text = text;
			this.Type = textState;
		}

		#endregion

		#region Overrides

		public override string ToString()
		{
			return Text;
		}

		#endregion

		#region Properties

		public TextState Type { get; set; }

		private string Text { get; set; }

		public GlyphRun RenderedText { get; private set; }

		public SolidColorBrush BackgroundBrush
		{
			get
			{
				switch (Type)
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
				switch (Type)
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

		#endregion

		#region Methods

		private double renderedTextWidth;
		private Typeface renderedTypeface;
		private double renderedFontSize;
		private double renderedDpiScale;
		private bool renderedWhiteSpace;
		private int renderedTabSize;

		public GlyphRun GetRenderedText(Typeface typeface, double fontSize, double dpiScale, bool whiteSpace, int tabSize, out double runWidth)
		{
			if (!typeface.Equals(renderedTypeface) || fontSize != renderedFontSize || dpiScale != renderedDpiScale || whiteSpace != renderedWhiteSpace || tabSize != renderedTabSize)
			{
				RenderedText = TextUtils.CreateGlyphRun(Text, typeface, fontSize, dpiScale, out renderedTextWidth);

				renderedTypeface = typeface;
				renderedFontSize = fontSize;
				renderedDpiScale = dpiScale;
				renderedWhiteSpace = whiteSpace;
				renderedTabSize = tabSize;
			}

			runWidth = renderedTextWidth;
			return RenderedText;
		}

		#endregion

	}
}
