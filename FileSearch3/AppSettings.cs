using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;
using System.Xml;

namespace FileSearch
{
	public static class AppSettings
	{

		#region Members

		private const string SETTINGS_DIRECTORY = "FileSearch3";
		private const string SETTINGS_FILE_NAME = "Settings.xml";

		private static SettingsData Settings = new SettingsData();

		#endregion

		#region Properies

		public static ObservableCollection<TextAttribute> IgnoredFolders
		{
			get { return Settings.IgnoredFolders; }
			set { Settings.IgnoredFolders = value; }
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

		public static double FolderRowHeight
		{
			get { return Settings.FolderRowHeight; }
			set { Settings.FolderRowHeight = value; }
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
			set { font = value; Settings.Font = value.ToString(); }
		}

		public static int FontSize
		{
			get { return Settings.FontSize; }
			set { Settings.FontSize = value; }
		}

		public static int TabSize
		{
			get { return Settings.TabSize; }
			set { Settings.TabSize = value; }
		}

		public static bool ShowWhiteSpaceCharacters
		{
			get { return Settings.ShowWhiteSpaceCharacters; }
			set { Settings.ShowWhiteSpaceCharacters = value; }
		}

		private static SolidColorBrush normalForeground;
		public static SolidColorBrush NormalForeground
		{
			get { return normalForeground; }
			set { normalForeground = value; Settings.NormalForeground = value.Color; }
		}

		private static SolidColorBrush normalBackground;
		public static SolidColorBrush NormalBackground
		{
			get { return normalBackground; }
			set { normalBackground = value; Settings.NormalBackground = value.Color; }
		}

		private static SolidColorBrush hitForeground;
		public static SolidColorBrush HitForeground
		{
			get { return hitForeground; }
			set { hitForeground = value; Settings.HitForeground = value.Color; }
		}

		private static SolidColorBrush hitBackground;
		public static SolidColorBrush HitBackground
		{
			get { return hitBackground; }
			set { hitBackground = value; Settings.HitBackground = value.Color; }
		}

		private static SolidColorBrush headerForeground;
		public static SolidColorBrush HeaderForeground
		{
			get { return headerForeground; }
			set { headerForeground = value; Settings.HeaderForeground = value.Color; }
		}

		private static SolidColorBrush headerBackground;
		public static SolidColorBrush HeaderBackground
		{
			get { return headerBackground; }
			set { headerBackground = value; Settings.HeaderBackground = value.Color; }
		}

		public static double NameColumnWidth { get; internal set; } = 300;

		public static double SizeColumnWidth { get; internal set; } = 70;

		public static double DateColumnWidth { get; internal set; } = 120;

		#endregion

		#region Methods

		internal static void ReadSettingsFromDisk()
		{
			string settingsPath = Path.Combine(Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), SETTINGS_DIRECTORY), SETTINGS_FILE_NAME);
			DataContractSerializer xmlSerializer = new DataContractSerializer(typeof(SettingsData));

			if (File.Exists(settingsPath))
			{
				using (var xmlReader = XmlReader.Create(settingsPath))
				{
					try
					{
						Settings = (SettingsData)xmlSerializer.ReadObject(xmlReader);
					}
					catch (Exception e)
					{
						MessageBox.Show(e.Message, "Error Parsing XML", MessageBoxButton.OK, MessageBoxImage.Error);
					}
				}
			}

			if (Settings == null)
			{
				Settings = new SettingsData();
			}

			UpdateCachedSettings();
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

				using (var xmlWriter = XmlWriter.Create(Path.Combine(settingsPath, SETTINGS_FILE_NAME), xmlWriterSettings))
				{
					xmlSerializer.WriteObject(xmlWriter, Settings);
				}
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message);
			}
		}

		private static void UpdateCachedSettings()
		{
			Font = new FontFamily(Settings.Font);

			NormalForeground = new SolidColorBrush(Settings.NormalForeground);
			NormalBackground = new SolidColorBrush(Settings.NormalBackground);

			HitForeground = new SolidColorBrush(Settings.HitForeground);
			HitBackground = new SolidColorBrush(Settings.HitBackground);

			HeaderForeground = new SolidColorBrush(Settings.HeaderForeground);
			HeaderBackground = new SolidColorBrush(Settings.HeaderBackground);
		}

		#endregion

	}
}
