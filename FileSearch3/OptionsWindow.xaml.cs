﻿using Microsoft.Win32;
using System;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FileSearch
{
	/// <summary>
	/// Interaction logic for OptionsWindow.xaml
	/// </summary>
	public partial class OptionsWindow : Window
	{

		#region Members

		readonly string regPath = @"Folder\shell\filesearch";
		readonly string shellexecutePath = $"\"{System.Reflection.Assembly.GetExecutingAssembly().Location}\" \"%1\"";

		Rectangle selectecRectangle;

		#endregion

		#region Constructor

		public OptionsWindow()
		{
			InitializeComponent();

			foreach (FontFamily family in Fonts.SystemFontFamilies.OrderBy(x => x.Source))
			{
				ComboBoxFont.Items.Add(family.Source);
			}

			if (IsAdministrator)
			{
				ShellExtensionPanel.IsEnabled = true;
				NotAdminLabel.Visibility = Visibility.Collapsed;

				AddShellExtensionsCheckBox.IsChecked = Registry.ClassesRoot.CreateSubKey(regPath + "\\command").GetValue("")?.ToString() == shellexecutePath;

				AddShellExtensionsCheckBox.Checked += AddShellExtensionsCheckBox_Checked;
				AddShellExtensionsCheckBox.Unchecked += AddShellExtensionsCheckBox_Unchecked;
			}
		}

		#endregion

		#region Properties

		public bool IsAdministrator
		{
			get
			{
				WindowsIdentity wi = WindowsIdentity.GetCurrent();
				WindowsPrincipal wp = new WindowsPrincipal(wi);

				return wp.IsInRole(WindowsBuiltInRole.Administrator);
			}
		}

		#endregion

		#region Methods

		private void CleanIgnores()
		{
			MainWindowViewModel viewModel = DataContext as MainWindowViewModel;

			for (int i = viewModel.IgnoredDirectories.Count - 1; i >= 0; i--)
			{
				if (string.IsNullOrWhiteSpace(viewModel.IgnoredDirectories[i].Text))
				{
					viewModel.IgnoredDirectories.RemoveAt(i);
				}
			}

			for (int i = viewModel.IgnoredFiles.Count - 1; i >= 0; i--)
			{
				if (string.IsNullOrWhiteSpace(viewModel.IgnoredFiles[i].Text))
				{
					viewModel.IgnoredFiles.RemoveAt(i);
				}
			}
		}

		#endregion

		#region Events

		private void AddShellExtensionsCheckBox_Unchecked(object sender, RoutedEventArgs e)
		{
			Registry.ClassesRoot.DeleteSubKeyTree(regPath);
		}

		private void AddShellExtensionsCheckBox_Checked(object sender, RoutedEventArgs e)
		{
			using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(regPath))
			{
				key.SetValue(null, "Search From Here");
			}
			using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(regPath + "\\command"))
			{
				key.SetValue(null, shellexecutePath);
			}
		}

		private void ButtonBrowseFont_Click(object sender, RoutedEventArgs e)
		{
			FontDialog fd = new FontDialog();
			fd.FontMustExist = true;

			if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				ComboBoxFont.Text = fd.Font.Name;
				TextBoxFontSize.Text = ((int)(fd.Font.Size * 96.0 / 72.0)).ToString(CultureInfo.InvariantCulture);
			}
		}

		private void Rectangle_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			selectecRectangle = e.Source as Rectangle;

			LabelA.Visibility = selectecRectangle == SelectionBackground ? Visibility.Visible : Visibility.Collapsed;
			SliderA.Visibility = selectecRectangle == SelectionBackground ? Visibility.Visible : Visibility.Collapsed;

			Color currentColor = Color.FromArgb((byte)(selectecRectangle == SelectionBackground ? ((SolidColorBrush)selectecRectangle.Fill).Color.A : 255), ((SolidColorBrush)selectecRectangle.Fill).Color.R, ((SolidColorBrush)selectecRectangle.Fill).Color.G, ((SolidColorBrush)selectecRectangle.Fill).Color.B);

			SliderR.Value = currentColor.R;
			SliderG.Value = currentColor.G;
			SliderB.Value = currentColor.B;
			SliderA.Value = currentColor.A;

			ColorHex.Text = currentColor.ToString();

			ColorChooser.IsOpen = true;
		}

		private void ButtonResetColors_Click(object sender, RoutedEventArgs e)
		{
			NormalForeground.Fill = new SolidColorBrush(DefaultSettings.NormalForeground);
			NormalBackground.Fill = new SolidColorBrush(DefaultSettings.NormalBackground);

			HitForeground.Fill = new SolidColorBrush(DefaultSettings.HitForeground);
			HitBackground.Fill = new SolidColorBrush(DefaultSettings.HitBackground);

			HeaderForeground.Fill = new SolidColorBrush(DefaultSettings.HeaderForeground);
			HeaderBackground.Fill = new SolidColorBrush(DefaultSettings.HeaderBackground);

			SelectionBackground.Fill = new SolidColorBrush(DefaultSettings.SelectionBackground);
		}

		private void ButtonResetFont_Click(object sender, RoutedEventArgs e)
		{
			ComboBoxFont.Text = DefaultSettings.Font;
			TextBoxFontSize.Text = DefaultSettings.FontSize.ToString();
		}

		private void ButtonOk_Click(object sender, RoutedEventArgs e)
		{
			CleanIgnores();
			DialogResult = true;
		}

		private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			byte alpha = (byte)(selectecRectangle == SelectionBackground ? (byte)SliderA.Value : 255);

			Color newColor = Color.FromArgb(alpha, (byte)SliderR.Value, (byte)SliderG.Value, (byte)SliderB.Value);
			ColorHex.Text = newColor.ToString();
			selectecRectangle.Fill = new SolidColorBrush(newColor);

			SliderR.Background = new LinearGradientBrush(Color.FromArgb(alpha, 0, newColor.G, newColor.B), Color.FromArgb(alpha, 255, newColor.G, newColor.B), 0);
			SliderG.Background = new LinearGradientBrush(Color.FromArgb(alpha, newColor.R, 0, newColor.B), Color.FromArgb(alpha, newColor.R, 255, newColor.B), 0);
			SliderB.Background = new LinearGradientBrush(Color.FromArgb(alpha, newColor.R, newColor.G, 0), Color.FromArgb(alpha, newColor.R, newColor.G, 255), 0);
			SliderA.Background = new LinearGradientBrush(Color.FromArgb(0, newColor.R, newColor.G, newColor.B), Color.FromArgb(255, newColor.R, newColor.G, newColor.B), 0);
		}

		#endregion

	}
}
