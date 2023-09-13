using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FileSearch;

public class HitMapControl : Control
{

	#region Members

	private double dpiScale = 0;

	#endregion

	#region Constructors

	static HitMapControl()
	{
		DefaultStyleKeyProperty.OverrideMetadata(typeof(HitMapControl), new FrameworkPropertyMetadata(typeof(HitMapControl)));
	}

	public HitMapControl()
	{
		this.ClipToBounds = true;
	}

	#endregion

	#region Overrides

	protected override void OnRender(DrawingContext drawingContext)
	{
		Debug.Print("DiffMap OnRender");

		// Fill background
		drawingContext.DrawRectangle(AppSettings.DialogBackground, null, new Rect(0, 0, this.ActualWidth, this.ActualHeight));

		if (Lines.Count == 0)
			return;

		Matrix m = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
		dpiScale = 1 / m.M11;

		double scrollableHeight = ActualHeight - (2 * RoundToWholePixels(SystemParameters.VerticalScrollBarButtonHeight));
		double lineHeight = scrollableHeight / Lines.Count;

		double lastHeight = -1;

		SolidColorBrush hitBrush = Darken(AppSettings.HitBackground, .85);
		SolidColorBrush headerBrush = Darken(AppSettings.HeaderBackground, .85);

		SolidColorBrush lineBrush;

		for (int i = 0; i < Lines.Count; i++)
		{
			Line line = Lines[i];

			switch (line.Type)
			{
				case TextState.Hit:
					lineBrush = hitBrush;
					break;

				case TextState.Header:
					lineBrush = headerBrush;
					break;

				default:
					continue;
			}

			int count = 1;

			while (i + count < Lines.Count && line.Type == Lines[i + count].Type)
			{
				count++;
			}

			Rect rect = new Rect(RoundToWholePixels(1), Math.Floor((i * lineHeight + SystemParameters.VerticalScrollBarButtonHeight) / dpiScale) * dpiScale, ActualWidth - RoundToWholePixels(2), Math.Ceiling(Math.Max(lineHeight * count, 1) / dpiScale) * dpiScale);

			if (rect.Bottom > lastHeight)
			{
				drawingContext.DrawRectangle(lineBrush, null, rect);

				lastHeight = rect.Bottom;
			}

			i += count - 1;
		}
	}

	#endregion

	#region Dependency Properties

	public static readonly DependencyProperty LinesProperty = DependencyProperty.Register("Lines", typeof(ObservableCollection<Line>), typeof(HitMapControl), new FrameworkPropertyMetadata(new ObservableCollection<Line>(), FrameworkPropertyMetadataOptions.AffectsRender));

	public ObservableCollection<Line> Lines
	{
		get { return (ObservableCollection<Line>)GetValue(LinesProperty); }
		set { SetValue(LinesProperty, value); }
	}


	public static readonly DependencyProperty UpdateTriggerProperty = DependencyProperty.Register("UpdateTrigger", typeof(int), typeof(HitMapControl), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

	public int UpdateTrigger
	{
		get { return (int)GetValue(UpdateTriggerProperty); }
		set { SetValue(UpdateTriggerProperty, value); }
	}

	#endregion

	#region Methods

	private SolidColorBrush Darken(SolidColorBrush color, double factor)
	{
		byte r = (byte)(color.Color.R * factor);
		byte g = (byte)(color.Color.G * factor);
		byte b = (byte)(color.Color.B * factor);
		return new SolidColorBrush(Color.FromRgb(r, g, b));
	}

	private double RoundToWholePixels(double x)
	{
		return Math.Round(x / dpiScale) * dpiScale;
	}

	#endregion

}
