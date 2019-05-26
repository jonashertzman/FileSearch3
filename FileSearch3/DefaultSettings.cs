using System.Windows.Media;

namespace FileSearch
{
	public static class DefaultSettings
	{
		internal static Color NormalForeground { get; } = Colors.Black;
		internal static Color NormalBackground { get; } = Colors.White;

		internal static Color HitForeground { get; } = Color.FromRgb(255, 255, 150);
		internal static Color HitBackground { get; } = Color.FromRgb(255, 160, 160);

		internal static Color HeaderForeground { get; } = Color.FromRgb(130, 130, 130);
		internal static Color HeaderBackground { get; } = Color.FromRgb(210, 210, 210);

		internal static Color SelectionBackground { get; } = Color.FromArgb(50, 0, 150, 210);

		internal static string Font { get; } = "Courier New";
		internal static int FontSize { get; } = 11;
		internal static int TabSize { get; } = 2;

	}
}