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

		HitForeground = "#FFFFB900",
		HitBackground = "#FF6F0012",

		HeaderForeground = "#FF6D6D6D",
		HeaderBackground = "#FF2C2C2C",

		LineNumberForeground = "#FF797979",
		CurrentHitBackground = "#FF252525",

		SelectionBackground = "#320096D2",

		// UI colors
		NormalText = "#FFD8D8D8",
		DisabledText = "#FF888888",

		DisabledBackground = "#FF444444",

		WindowBackground = "#FF0B0B0B",
		DialogBackground = "#FF171717",

		ControlLightBackground = "#FF1B1B1B",
		ControlDarkBackground = "#FF3F3F3F",

		BorderLight = "#FF323232",
		BorderDark = "#FF636363",

		HighlightBackground = "#FF112E3C",
		HighlightBorder = "#FF2F7999",

		AttentionBackground = "#FF5C2626",
	};

	internal static ColorTheme LightTheme { get; } = new ColorTheme()
	{
		Name = "Light",

		// Editor colors
		NormalForeground = "#FF000000",
		NormalBackground = "#FFFFFFFF",

		HitForeground = "#FFFFF300",
		HitBackground = "#FFFF8689",

		HeaderForeground = "#FF6A6A6A",
		HeaderBackground = "#FFC2C2C2",

		LineNumberForeground = "#FF585858",
		CurrentHitBackground = "#FFB7B7B7",

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
