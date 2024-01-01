namespace FileSearch;

public static class DefaultSettings
{

	internal static string Font { get; } = "Courier New";
	internal static int FontSize { get; } = 11;
	internal static int TabSize { get; } = 2;

	internal static ColorTheme DarkTheme { get; } = new ColorTheme()
	{
		Name = "Dark",

		// Editor colors
		NormalForeground = "#FFD8D8D8",
		NormalBackground = "#1B1B1B",

		HitForeground = "#FF9698FF",
		HitBackground = "#FF2B293A",

		HeaderForeground = "#FFFF8387",
		HeaderBackground = "#FF5C2626",

		LineNumberColor = "#FF797979",
		CurrentDiffColor = "#FF252525",
		SelectionBackground = "#320096D2",

		// UI colors
		NormalText = "#FFD8D8D8",
		DisabledText = "#FF888888",

		DisabledBackground = "#FF444444",

		WindowBackground = "#FF0B0B0B",
		DialogBackground = "#FF171717",

		ControlLightBackground = "#FF262626",
		ControlDarkBackground = "#FF3F3F3F",

		BorderLight = "#FF323232",
		BorderDark = "#FF595959",

		HighlightBackground = "#FF112E3C",
		HighlightBorder = "#2F7999",

		AttentionBackground = "#FF5C2626",
	};

	internal static ColorTheme LightTheme { get; } = new ColorTheme()
	{
		Name = "Light",

		// Editor colors
		NormalForeground = "#FF000000",
		NormalBackground = "#FFFFFFFF",

		HitForeground = "#FF000000",
		HitBackground = "#FFDCDCFF",

		HeaderForeground = "#FFC80000",
		HeaderBackground = "#FFFFDCDC",

		LineNumberColor = "#FF585858",
		CurrentDiffColor = "#FFB7B7B7",
		SelectionBackground = "#320096D2",

		// UI colors
		NormalText = "#FF000000",
		DisabledText = "#FF888888",

		DisabledBackground = "#FFAAAAAA",

		WindowBackground = "#FFFFFFFF",
		DialogBackground = "#FFEBEBEB",

		ControlLightBackground = "#FFFFFFFF",
		ControlDarkBackground = "#FFD9D9D9",

		BorderLight = "#FFCFCFCF",
		BorderDark = "#FFAAAAAA",

		HighlightBackground = "#FFDCECFC",
		HighlightBorder = "#FF7EB4EA",

		AttentionBackground = "#FFFF9F9D",
	};

}
