using System;
using System.Windows;
using System.Windows.Controls;

namespace FileSearch
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{

		#region Constructor

		public MainWindow()
		{
			InitializeComponent();
			DataContext = Stuff.SettingsData;
		}

		#endregion

		#region Properties

		public SearchInstance ActiveSearch
		{
			get
			{
				return Stuff.SettingsData.ActiveSearchInstance;
			}
		}

		#endregion

		#region Events

		private void Window_Closed(object sender, EventArgs e)
		{
			Stuff.WriteSettingsToDisk();
		}

		private void Window_Initialized(object sender, EventArgs e)
		{
			Stuff.ReadSettingsFromDisk();
			if (Stuff.SettingsData.SearchInstances.Count == 0)
			{
				AddNewSearch();
			}
		}

		private void ButtonNewSearchInstance_Click(object sender, RoutedEventArgs e)
		{
			AddNewSearch();
		}

		private void TabButton_Click(object sender, RoutedEventArgs e)
		{
			SetActiveTab((SearchInstance)((Button)e.Source).Tag);
		}

		private void ToolbarButtonDeleteSearch_Click(object sender, RoutedEventArgs e)
		{
			int removedIndex = Stuff.SettingsData.SearchInstances.IndexOf(ActiveSearch);
			Stuff.SettingsData.SearchInstances.RemoveAt(removedIndex);
			if (Stuff.SettingsData.SearchInstances.Count > 0)
			{
				SetActiveTab(Stuff.SettingsData.SearchInstances[Math.Max(0, removedIndex - 1)]);
			}
			else
			{
				AddNewSearch();
			}
		}

		#endregion

		#region Methods

		private void AddNewSearch()
		{
			SearchInstance newInstance = new SearchInstance();
			Stuff.SettingsData.SearchInstances.Add(newInstance);
			SetActiveTab(newInstance);
		}

		private void SetActiveTab(SearchInstance searchInstance)
		{
			foreach (SearchInstance s in Stuff.SettingsData.SearchInstances)
			{
				s.IsSelected = s == searchInstance;
			}
			Stuff.SettingsData.ActiveSearchInstance = searchInstance;
		}

		#endregion

	}
}
