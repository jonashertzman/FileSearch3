using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace FileSearch
{
	public class SettingsData
	{
		public ObservableCollection<TextAttribute> IgnoredFolders { get; set; } = new ObservableCollection<TextAttribute>();

		public ObservableCollection<TextAttribute> IgnoredFiles { get; set; } = new ObservableCollection<TextAttribute>();

		public ObservableCollection<SearchInstance> SearchInstances { get; set; } = new ObservableCollection<SearchInstance>();

		public Color FullMatchForeground { get; set; } = DefaultSettings.FullMatchForeground;
		public Color FullMatchBackground { get; set; } = DefaultSettings.FullMatchBackground;

		public Color PartialMatchForeground { get; set; } = DefaultSettings.PartialMatchForeground;
		public Color PartialMatchBackground { get; set; } = DefaultSettings.PartialMatchBackground;

		public Color DeletedForeground { get; set; } = DefaultSettings.DeletedForeground;
		public Color DeletedBackground { get; set; } = DefaultSettings.DeletedBackground;

		public Color NewForeground { get; set; } = DefaultSettings.NewForeground;
		public Color NewBackground { get; set; } = DefaultSettings.NewBackground;

		public Color IgnoredForeground { get; set; } = DefaultSettings.IgnoredForeground;
		public Color IgnoredBackground { get; set; } = DefaultSettings.IgnoredBackground;

		public string Font { get; set; } = DefaultSettings.Font;
		public int FontSize { get; set; } = DefaultSettings.FontSize;
		public int TabSize { get; set; } = DefaultSettings.TabSize;

		public bool ShowWhiteSpaceCharacters { get; set; } = false;
		
		public double PositionLeft { get; set; }
		public double PositionTop { get; set; }
		public double Width { get; set; } = 700;
		public double Height { get; set; } = 500;
		public double FolderRowHeight { get; set; } = 300;
		public WindowState WindowState { get; set; }


	}
}
