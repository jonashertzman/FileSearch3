using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;
using System.Xml;

namespace FileSearch;

public static class AppSettings
{

	#region Members

	private const string SETTINGS_DIRECTORY = "FileSearch3";
	private const string SETTINGS_FILE_NAME = "Settings.xml";

	private static SettingsData Settings = new SettingsData();

	#endregion

	#region Properies

	public static string Id
	{
		get { return Settings.Id; }
	}

	public static DateTime LastUpdateTime
	{
		get { return Settings.LastUpdateTime; }
		set { Settings.LastUpdateTime = value; }
	}

	public static bool CheckForUpdates
	{
		get { return Settings.CheckForUpdates; }
		set { Settings.CheckForUpdates = value; }
	}


	public static ObservableCollection<TextAttribute> IgnoredDirectories
	{
		get { return Settings.IgnoredDirectories; }
		set { Settings.IgnoredDirectories = value; }
	}

	public static ObservableCollection<TextAttribute> IgnoredFiles
	{
		get { return Settings.IgnoredFiles; }
		set { Settings.IgnoredFiles = value; }
	}

	public static ObservableCollection<SearchInstance> SearchInstances
	{
		get { return Settings.SearchInstances; }
		set { Settings.SearchInstances = value; }
	}

	public static double PositionLeft
	{
		get { return Settings.PositionLeft; }
		set { Settings.PositionLeft = value; }
	}

	public static double PositionTop
	{
		get { return Settings.PositionTop; }
		set { Settings.PositionTop = value; }
	}

	public static double Width
	{
		get { return Settings.Width; }
		set { Settings.Width = value; }
	}

	public static double Height
	{
		get { return Settings.Height; }
		set { Settings.Height = value; }
	}

	public static WindowState WindowState
	{
		get { return Settings.WindowState; }
		set { Settings.WindowState = value; }
	}

	private static FontFamily font;
	public static FontFamily Font
	{
		get { return font; }
		set
		{
			font = value;
			Settings.Font = value.ToString();
		}
	}

	public static int FontSize
	{
		get { return Settings.FontSize; }
		set { Settings.FontSize = value; }
	}

	public static int Zoom
	{
		get { return Settings.Zoom; }
		set { Settings.Zoom = value; }
	}

	public static int TabSize
	{
		get { return Settings.TabSize; }
		set { Settings.TabSize = value; }
	}

	public static Themes Theme
	{
		get { return Settings.Theme; }
		set
		{
			Settings.Theme = value;

			UpdateCachedSettings();
			NotifyStaticPropertyChanged(nameof(WindowForegroundColor));
			NotifyStaticPropertyChanged(nameof(WindowBackgroundColor));
			NotifyStaticPropertyChanged(nameof(DisabledForegroundColor));
			NotifyStaticPropertyChanged(nameof(DisabledBackgroundColor));
			NotifyStaticPropertyChanged(nameof(DialogBackgroundColor));
			NotifyStaticPropertyChanged(nameof(ControlLightBackgroundColor));
			NotifyStaticPropertyChanged(nameof(BorderForegroundColor));
		}
	}

	public static ColorTheme CurrentTheme
	{
		get
		{
			return Theme switch
			{
				Themes.Light => Settings.LightTheme,
				Themes.Dark => Settings.DarkTheme,
				_ => throw new NotImplementedException(),
			};
		}
	}

	public static bool ShowWhiteSpaceCharacters
	{
		get { return Settings.ShowWhiteSpaceCharacters; }
		set { Settings.ShowWhiteSpaceCharacters = value; }
	}


	// Editor colors
	private static SolidColorBrush normalForeground;
	public static SolidColorBrush NormalForeground
	{
		get { return normalForeground; }
		set
		{
			normalForeground = value;
			CurrentTheme.NormalForeground = value.Color.ToString();
		}
	}

	private static SolidColorBrush normalBackground;
	public static SolidColorBrush NormalBackground
	{
		get { return normalBackground; }
		set
		{
			normalBackground = value;
			CurrentTheme.NormalBackground = value.Color.ToString();
		}
	}

	private static SolidColorBrush hitForeground;
	public static SolidColorBrush HitForeground
	{
		get { return hitForeground; }
		set
		{
			hitForeground = value;
			CurrentTheme.HitForeground = value.Color.ToString();
		}
	}

	private static SolidColorBrush hitBackground;
	public static SolidColorBrush HitBackground
	{
		get { return hitBackground; }
		set
		{
			hitBackground = value;
			CurrentTheme.HitBackground = value.Color.ToString();
		}
	}

	private static SolidColorBrush headerForeground;
	public static SolidColorBrush HeaderForeground
	{
		get { return headerForeground; }
		set
		{
			headerForeground = value;
			CurrentTheme.HeaderForeground = value.Color.ToString();
		}
	}

	private static SolidColorBrush headerBackground;
	public static SolidColorBrush HeaderBackground
	{
		get { return headerBackground; }
		set
		{
			headerBackground = value;
			CurrentTheme.HeaderBackground = value.Color.ToString();
		}
	}

	private static SolidColorBrush selectionBackground;
	public static SolidColorBrush SelectionBackground
	{
		get { return selectionBackground; }
		set
		{
			selectionBackground = value;
			CurrentTheme.SelectionBackground = value.Color.ToString();
		}
	}

	private static SolidColorBrush lineNumberColor;
	public static SolidColorBrush LineNumberColor
	{
		get { return lineNumberColor; }
		set
		{
			lineNumberColor = value;
			lineNumberColor.Freeze();
			CurrentTheme.LineNumberColor = value.Color.ToString();
		}
	}


	// UI colors
	private static SolidColorBrush windowForeground = DefaultSettings.LightTheme.NormalText.ToBrush();
	public static SolidColorBrush WindowForeground
	{
		get { return windowForeground; }
		set
		{
			windowForeground = value;
			windowForeground.Freeze();
			CurrentTheme.NormalText = value.Color.ToString();
			NotifyStaticPropertyChanged(nameof(WindowForeground));
			NotifyStaticPropertyChanged(nameof(WindowForegroundColor));
		}
	}
	public static Color WindowForegroundColor
	{
		get { return WindowForeground.Color; }
	}

	private static SolidColorBrush disabledForeground = DefaultSettings.LightTheme.DisabledText.ToBrush();
	public static SolidColorBrush DisabledForeground
	{
		get { return disabledForeground; }
		set
		{
			disabledForeground = value;
			disabledForeground.Freeze();
			CurrentTheme.DisabledText = value.Color.ToString();
			NotifyStaticPropertyChanged(nameof(DisabledForeground));
			NotifyStaticPropertyChanged(nameof(DisabledForegroundColor));
		}
	}
	public static Color DisabledForegroundColor
	{
		get { return DisabledForeground.Color; }
	}

	private static SolidColorBrush disabledBackground = DefaultSettings.LightTheme.DisabledBackground.ToBrush();
	public static SolidColorBrush DisabledBackground
	{
		get { return disabledBackground; }
		set
		{
			disabledBackground = value;
			disabledBackground.Freeze();
			CurrentTheme.DisabledBackground = value.Color.ToString();
			NotifyStaticPropertyChanged(nameof(DisabledBackground));
			NotifyStaticPropertyChanged(nameof(DisabledBackgroundColor));
		}
	}
	public static Color DisabledBackgroundColor
	{
		get { return DisabledBackground.Color; }
	}

	private static SolidColorBrush windowBackground = DefaultSettings.LightTheme.WindowBackground.ToBrush();
	public static SolidColorBrush WindowBackground
	{
		get { return windowBackground; }
		set
		{
			windowBackground = value;
			windowBackground.Freeze();
			CurrentTheme.WindowBackground = value.Color.ToString();
			NotifyStaticPropertyChanged(nameof(WindowBackground));
			NotifyStaticPropertyChanged(nameof(WindowBackgroundColor));
		}
	}
	public static Color WindowBackgroundColor
	{
		get { return windowBackground.Color; }
	}

	private static SolidColorBrush dialogBackground = DefaultSettings.LightTheme.DialogBackground.ToBrush();
	public static SolidColorBrush DialogBackground
	{
		get { return dialogBackground; }
		set
		{
			dialogBackground = value;
			dialogBackground.Freeze();
			CurrentTheme.DialogBackground = value.Color.ToString();
			NotifyStaticPropertyChanged(nameof(DialogBackground));
			NotifyStaticPropertyChanged(nameof(DialogBackgroundColor));
		}
	}
	public static Color DialogBackgroundColor
	{
		get { return dialogBackground.Color; }
	}

	private static SolidColorBrush controlLightBackground = DefaultSettings.LightTheme.ControlLightBackground.ToBrush();
	public static SolidColorBrush ControlLightBackground
	{
		get { return controlLightBackground; }
		set
		{
			controlLightBackground = value;
			controlLightBackground.Freeze();
			CurrentTheme.ControlLightBackground = value.Color.ToString();
			NotifyStaticPropertyChanged(nameof(ControlLightBackground));
			NotifyStaticPropertyChanged(nameof(ControlLightBackgroundColor));
		}
	}
	public static Color ControlLightBackgroundColor
	{
		get { return controlLightBackground.Color; }
	}

	private static SolidColorBrush controlDarkBackground = DefaultSettings.LightTheme.ControlDarkBackground.ToBrush();
	public static SolidColorBrush ControlDarkBackground
	{
		get { return controlDarkBackground; }
		set
		{
			controlDarkBackground = value;
			controlDarkBackground.Freeze();
			CurrentTheme.ControlDarkBackground = value.Color.ToString();
			NotifyStaticPropertyChanged(nameof(ControlDarkBackground));
			NotifyStaticPropertyChanged(nameof(ControlDarkBackgroundColor));
		}
	}
	public static Color ControlDarkBackgroundColor
	{
		get { return controlDarkBackground.Color; }
	}

	private static SolidColorBrush borderForeground = DefaultSettings.LightTheme.BorderLight.ToBrush();
	public static SolidColorBrush BorderForeground
	{
		get { return borderForeground; }
		set
		{
			borderForeground = value;
			borderForeground.Freeze();
			CurrentTheme.BorderLight = value.Color.ToString();
			NotifyStaticPropertyChanged(nameof(BorderForeground));
			NotifyStaticPropertyChanged(nameof(BorderForegroundColor));
		}
	}
	public static Color BorderForegroundColor
	{
		get { return borderForeground.Color; }
	}

	private static SolidColorBrush borderDarkForeground = DefaultSettings.LightTheme.BorderDark.ToBrush();
	public static SolidColorBrush BorderDarkForeground
	{
		get { return borderDarkForeground; }
		set
		{
			borderDarkForeground = value;
			borderDarkForeground.Freeze();
			CurrentTheme.BorderDark = value.Color.ToString();
			NotifyStaticPropertyChanged(nameof(BorderDarkForeground));
			NotifyStaticPropertyChanged(nameof(BorderDarkForegroundColor));
		}
	}
	public static Color BorderDarkForegroundColor
	{
		get { return borderDarkForeground.Color; }
	}

	private static SolidColorBrush highlightBackground = DefaultSettings.LightTheme.HighlightBackground.ToBrush();
	public static SolidColorBrush HighlightBackground
	{
		get { return highlightBackground; }
		set
		{
			highlightBackground = value;
			highlightBackground.Freeze();
			CurrentTheme.HighlightBackground = value.Color.ToString();
			NotifyStaticPropertyChanged(nameof(HighlightBackground));
			NotifyStaticPropertyChanged(nameof(HighlightBackgroundColor));
		}
	}
	public static Color HighlightBackgroundColor
	{
		get { return highlightBackground.Color; }
	}

	private static SolidColorBrush highlightBorder = DefaultSettings.LightTheme.HighlightBorder.ToBrush();
	public static SolidColorBrush HighlightBorder
	{
		get { return highlightBorder; }
		set
		{
			highlightBorder = value;
			highlightBorder.Freeze();
			CurrentTheme.HighlightBorder = value.Color.ToString();
			NotifyStaticPropertyChanged(nameof(HighlightBorder));
			NotifyStaticPropertyChanged(nameof(HighlightBorderColor));
		}
	}
	public static Color HighlightBorderColor
	{
		get { return highlightBorder.Color; }
	}

	private static SolidColorBrush attentionBackground = DefaultSettings.LightTheme.AttentionBackground.ToBrush();
	public static SolidColorBrush AttentionBackground
	{
		get { return attentionBackground; }
		set
		{
			attentionBackground = value;
			attentionBackground.Freeze();
			CurrentTheme.AttentionBackground = value.Color.ToString();
			NotifyStaticPropertyChanged(nameof(AttentionBackground));
			NotifyStaticPropertyChanged(nameof(AttentionBackgroundColor));
		}
	}
	public static Color AttentionBackgroundColor
	{
		get { return attentionBackground.Color; }
	}



	public static double PhraseGridHeight
	{
		get { return Settings.PhraseGridHeight; }
		set { Settings.PhraseGridHeight = value; }
	}

	public static double DirectoriesGridHeight
	{
		get { return Settings.DirectoriesGridHeight; }
		set { Settings.DirectoriesGridHeight = value; }
	}

	public static double SearchAttributesWidth
	{
		get { return Settings.SearchAttributesWidth; }
		set { Settings.SearchAttributesWidth = value; }
	}

	public static double FileListHeight
	{
		get { return Settings.FileListHeight; }
		set { Settings.FileListHeight = value; }
	}

	#endregion

	#region Methods

	internal static void LoadSettings()
	{
		SettingsData storedSettings = ReadSettingsFromDisk();

		if (storedSettings != null)
		{
			MergeSettings(Settings, storedSettings);
		}

		UpdateCachedSettings();
	}

	private static void MergeSettings(object source, object addition)
	{
		foreach (var property in addition.GetType().GetProperties())
		{
			if (property.PropertyType.Name == nameof(ColorTheme))
			{
				MergeSettings(property.GetValue(source), property.GetValue(addition));
			}
			else
			{
				if (property.GetValue(addition) != null)
				{
					property.SetValue(source, property.GetValue(addition));
				}
			}
		}
	}

	private static SettingsData ReadSettingsFromDisk()
	{
		string settingsPath = Path.Combine(Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), SETTINGS_DIRECTORY), SETTINGS_FILE_NAME);
		DataContractSerializer xmlSerializer = new DataContractSerializer(typeof(SettingsData));

		if (File.Exists(settingsPath))
		{
			using var xmlReader = XmlReader.Create(settingsPath);
			try
			{
				return (SettingsData)xmlSerializer.ReadObject(xmlReader);
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message, "Error Parsing Settings File", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		return null;
	}

	internal static void WriteSettingsToDisk()
	{
		try
		{
			string settingsPath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), SETTINGS_DIRECTORY);

			DataContractSerializer xmlSerializer = new DataContractSerializer(typeof(SettingsData));
			var xmlWriterSettings = new XmlWriterSettings { Indent = true, IndentChars = " " };

			if (!Directory.Exists(settingsPath))
			{
				Directory.CreateDirectory(settingsPath);
			}

			using var xmlWriter = XmlWriter.Create(Path.Combine(settingsPath, SETTINGS_FILE_NAME), xmlWriterSettings);

			xmlSerializer.WriteObject(xmlWriter, Settings);
		}
		catch (Exception e)
		{
			MessageBox.Show(e.Message);
		}
	}

	private static void UpdateCachedSettings()
	{
		Font = new FontFamily(Settings.Font);

		// Editor colors

		NormalForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.NormalForeground));
		NormalBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.NormalBackground));

		HitForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.HitForeground));
		HitBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.HitBackground));

		HeaderForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.HeaderForeground));
		HeaderBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.HeaderBackground));

		LineNumberColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.LineNumberColor));
		SelectionBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.SelectionBackground));

		// UI colors
		WindowForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.NormalText));
		DisabledForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.DisabledText));

		DisabledBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.DisabledBackground));

		WindowBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.WindowBackground));
		DialogBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.DialogBackground));

		ControlLightBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.ControlLightBackground));
		ControlDarkBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.ControlDarkBackground));

		BorderForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.BorderLight));
		BorderDarkForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.BorderDark));

		HighlightBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.HighlightBackground));
		HighlightBorder = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.HighlightBorder));

		AttentionBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.AttentionBackground));
	}

	#endregion

	#region NotifyStaticPropertyChanged

	public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

	private static void NotifyStaticPropertyChanged(string propertyName)
	{
		StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
	}

	#endregion

}
