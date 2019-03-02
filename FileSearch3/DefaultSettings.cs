using System.Windows.Media;

namespace FileSearch
{
	public static class DefaultSettings
	{

		internal static Color NormalTextForeground { get; } = Colors.Black;
		internal static Color NormalTextBackground { get; } = Colors.White;

		internal static Color HitTextForeground { get; } = Color.FromRgb(255, 255, 0);
		internal static Color HitTextBackground { get; } = Color.FromRgb(255, 100, 100);

		internal static Color HeaderTextForeground { get; } = Color.FromRgb(130, 130, 130);
		internal static Color HeaderTextBackground { get; } = Color.FromRgb(210, 210, 210);

		internal static string Font { get; } = "Courier New";
		internal static int FontSize { get; } = 11;
		internal static int TabSize { get; } = 2;

	}
}