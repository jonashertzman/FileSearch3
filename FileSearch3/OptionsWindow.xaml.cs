using Microsoft.Win32;
using System.IO;
using System.Security.Principal;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FileSearch;

public partial class OptionsWindow : Window
{

	#region Members

	readonly string regPath = @"Folder\shell\filesearch";
	readonly string shellexecutePath = $"\"{new FileInfo(Process.GetCurrentProcess().MainModule.FileName)}\" \"%1\"";

	Rectangle selectecRectangle;

	#endregion

	#region Constructor

	public OptionsWindow()
	{
		InitializeComponent();

		Utils.HideMinimizeAndMaximizeButtons(this);

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
		var Default = AppSettings.Theme switch
		{
			Themes.Light => DefaultSettings.LightTheme,
			Themes.Dark => DefaultSettings.DarkTheme,
			_ => throw new NotImplementedException(),
		};

		//// Folder diff colors
		//FolderFullMatchForeground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.FolderFullMatchForeground));
		//FolderFullMatchBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.FolderFullMatchBackground));

		//FolderPartialMatchForeground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.FolderPartialMatchForeground));
		//FolderPartialMatchBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.FolderPartialMatchBackground));

		//FolderDeletedForeground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.FolderDeletedForeground));
		//FolderDeletedBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.FolderDeletedBackground));

		//FolderNewForeground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.FolderNewForeground));
		//FolderNewBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.FolderNewBackground));

		//FolderIgnoredForeground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.FolderIgnoredForeground));
		//FolderIgnoredBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.FolderIgnoredBackground));

		//// File diff colors
		//FullMatchForeground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.FullMatchForeground));
		//FullMatchBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.FullMatchBackground));

		//PartialMatchForeground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.PartialMatchForeground));
		//PartialMatchBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.PartialMatchBackground));

		//DeletedForeground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.DeletedForeground));
		//DeletedBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.DeletedBackground));

		//NewForeground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.NewForeground));
		//NewBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.NewBackground));

		//IgnoredForeground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.IgnoredForeground));
		//IgnoredBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.IgnoredBackground));

		//MovedFromBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.MovedFromBackground));
		//MovedToBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.MovedToBackground));

		//LineNumber.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.LineNumberColor));
		//CurrentDiff.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.CurrentDiffColor));
		//Snake.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.SnakeColor));

		SelectionBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.SelectionBackground));

		// UI colors
		WindowForeground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.NormalText));
		DisabledForeground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.DisabledText));

		WindowBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.WindowBackground));
		DialogBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.DialogBackground));

		ControlLightBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.ControlLightBackground));
		ControlDarkBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.ControlDarkBackground));

		BorderForegroundx.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.BorderLight));
		BorderDarkForegroundx.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.BorderDark));

		HighlightBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.HighlightBackground));
		HighlightBorder.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.HighlightBorder));

		AttentionBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString((Default.AttentionBackground)));
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
