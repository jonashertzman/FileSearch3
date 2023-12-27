using System.Collections.ObjectModel;
using System.Windows;

namespace FileSearch;

public class SettingsData
{

	public string Id { get; set; } = Guid.NewGuid().ToString();

	public DateTime LastUpdateTime { get; set; } = DateTime.MinValue;
	public bool CheckForUpdates { get; set; } = true;

	public ObservableCollection<TextAttribute> IgnoredDirectories { get; set; } = [];
	public ObservableCollection<TextAttribute> IgnoredFiles { get; set; } = [];
	public ObservableCollection<SearchInstance> SearchInstances { get; set; } = [];

	public string Font { get; set; } = DefaultSettings.Font;
	public int FontSize { get; set; } = DefaultSettings.FontSize;
	public int Zoom { get; set; } = 0;
	public int TabSize { get; set; } = DefaultSettings.TabSize;

	public Themes Theme { get; set; } = Themes.Light;
	public ColorTheme DarkTheme { get; set; } = DefaultSettings.DarkTheme.Clone();
	public ColorTheme LightTheme { get; set; } = DefaultSettings.LightTheme.Clone();

	public bool ShowWhiteSpaceCharacters { get; set; } = false;

	public double PositionLeft { get; set; }
	public double PositionTop { get; set; }
	public double Width { get; set; } = 750;
	public double Height { get; set; } = 500;
	public WindowState WindowState { get; set; }

	public double PhraseGridHeight { get; set; } = 120;
	public double DirectoriesGridHeight { get; set; } = 120;
	public double FileListHeight { get; set; } = 150;
	public double SearchAttributesWidth { get; set; } = 150;

}
