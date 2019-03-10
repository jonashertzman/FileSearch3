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

		public Color NormalForeground { get; set; } = DefaultSettings.NormalForeground;
		public Color NormalBackground { get; set; } = DefaultSettings.NormalBackground;

		public Color HitForeground { get; set; } = DefaultSettings.HitForeground;
		public Color HitBackground { get; set; } = DefaultSettings.HitBackground;

		public Color HeaderForeground { get; set; } = DefaultSettings.HeaderForeground;
		public Color HeaderBackground { get; set; } = DefaultSettings.HeaderBackground;

		public Color SelectionBackground { get; set; } = DefaultSettings.SelectionBackground;

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
