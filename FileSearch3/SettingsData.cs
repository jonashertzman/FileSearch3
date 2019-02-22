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

		public Color NormalTextForeground { get; set; } = DefaultSettings.NormalTextForeground;
		public Color NormalTextBackground { get; set; } = DefaultSettings.NormalTextBackground;

		public Color HitTextForeground { get; set; } = DefaultSettings.HitTextForeground;
		public Color HitTextBackground { get; set; } = DefaultSettings.HitTextBackground;

		public Color HeaderTextForeground { get; set; } = DefaultSettings.HeaderTextForeground;
		public Color HeaderTextBackground { get; set; } = DefaultSettings.HeaderTextBackground;

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
