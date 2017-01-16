using System;
using System.IO;
using System.Runtime.Serialization;
using System.Windows;
using System.Xml;

namespace FileSearch
{
	public static class Stuff
	{

		#region Members

		static AppSettings appSettings;

		const string SETTINGS_DIRECTORY = "FileSearchWpf";
		const string SETTINGS_FILE_NAME = "Settings.xml";

		#endregion

		#region Properties

		public static AppSettings SettingsData
		{
			get
			{
				return appSettings;
			}
		}

		#endregion

		#region Methods

		internal static void ReadSettingsFromDisk()
		{
			string settingsPath = Path.Combine(Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), SETTINGS_DIRECTORY), SETTINGS_FILE_NAME);
			DataContractSerializer xmlSerializer = new DataContractSerializer(typeof(AppSettings));

			if (File.Exists(settingsPath))
			{
				using (var xmlReader = XmlReader.Create(settingsPath))
				{
					try
					{
						appSettings = (AppSettings)xmlSerializer.ReadObject(xmlReader);
					}
					catch (Exception e)
					{
						MessageBox.Show(e.Message, "Error Parsing XML", MessageBoxButton.OK, MessageBoxImage.Error);
					}
				}
			}

			if (appSettings == null)
			{
				appSettings = new AppSettings();
			}
		}

		internal static void WriteSettingsToDisk()
		{
			try
			{
				string settingsPath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), SETTINGS_DIRECTORY);

				DataContractSerializer xmlSerializer = new DataContractSerializer(typeof(AppSettings));
				var xmlWriterSettings = new XmlWriterSettings { Indent = true, IndentChars = " " };

				if (!Directory.Exists(settingsPath))
				{
					Directory.CreateDirectory(settingsPath);
				}

				using (var xmlWriter = XmlWriter.Create(Path.Combine(settingsPath, SETTINGS_FILE_NAME), xmlWriterSettings))
				{
					xmlSerializer.WriteObject(xmlWriter, appSettings);
				}
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message);
			}
		}

		#endregion

	}
}
