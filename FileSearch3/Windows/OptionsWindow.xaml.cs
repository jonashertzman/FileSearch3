using Microsoft.Win32;
using System.IO;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FileSearch;

public partial class OptionsWindow : Window
{

	#region Members

	readonly string regPath = @"Folder\shell\filesearch";
	readonly string shellExecutePath = $"\"{new FileInfo(Environment.ProcessPath)}\" \"%1\"";

	Rectangle selectedRectangle;

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

		foreach (string name in Enum.GetNames(typeof(Themes)))
		{
			ComboBoxTheme.Items.Add(new ComboBoxItem { Content = name });
		}

		if (IsAdministrator)
		{
			AddShellExtensionsCheckBox.IsEnabled = true;
			AddShellExtensionsCheckBox.Foreground = AppSettings.NormalForeground;
			NotAdminLabel.Visibility = Visibility.Collapsed;

			AddShellExtensionsCheckBox.IsChecked = Registry.ClassesRoot.CreateSubKey(regPath + "\\command").GetValue("")?.ToString() == shellExecutePath;

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
			WindowsPrincipal wp = new(wi);

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
			key.SetValue(null, shellExecutePath);
		}
	}

	private void Rectangle_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
	{
		selectedRectangle = e.Source as Rectangle;

		LabelA.Visibility = selectedRectangle == SelectionBackground ? Visibility.Visible : Visibility.Collapsed;
		SliderA.Visibility = selectedRectangle == SelectionBackground ? Visibility.Visible : Visibility.Collapsed;

		Color currentColor = Color.FromArgb((byte)(selectedRectangle == SelectionBackground ? ((SolidColorBrush)selectedRectangle.Fill).Color.A : 255), ((SolidColorBrush)selectedRectangle.Fill).Color.R, ((SolidColorBrush)selectedRectangle.Fill).Color.G, ((SolidColorBrush)selectedRectangle.Fill).Color.B);

		SliderR.Value = currentColor.R;
		SliderG.Value = currentColor.G;
		SliderB.Value = currentColor.B;
		SliderA.Value = currentColor.A;

		ColorHex.Text = currentColor.ToString();

		ColorChooser.IsOpen = true;
		SliderR.Focus();
	}

	private void ButtonResetColors_Click(object sender, RoutedEventArgs e)
	{
		var Default = AppSettings.Theme switch
		{
			Themes.Light => DefaultSettings.LightTheme,
			Themes.Dark => DefaultSettings.DarkTheme,
			_ => throw new NotImplementedException(),
		};

		// Hit colors
		NormalForeground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.NormalForeground));
		NormalBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.NormalBackground));

		HitForeground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.HitForeground));
		HitBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.HitBackground));

		HeaderForeground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.HeaderForeground));
		HeaderBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.HeaderBackground));

		LineNumberForeground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.LineNumberForeground));
		CurrentHitBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.CurrentHitBackground));

		SelectionBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.SelectionBackground));

		// UI colors
		WindowForeground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.NormalText));
		DisabledForeground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.DisabledText));

		DisabledBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.DisabledBackground));

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
		byte alpha = (byte)(selectedRectangle == SelectionBackground ? (byte)SliderA.Value : 255);

		if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
		{
			SliderR.Value = e.NewValue;
			SliderG.Value = e.NewValue;
			SliderB.Value = e.NewValue;
		}

		Color newColor = Color.FromArgb(alpha, (byte)SliderR.Value, (byte)SliderG.Value, (byte)SliderB.Value);
		ColorHex.Text = newColor.ToString();
		selectedRectangle.Fill = new SolidColorBrush(newColor);

		SliderR.Background = new LinearGradientBrush(Color.FromArgb(alpha, 0, newColor.G, newColor.B), Color.FromArgb(alpha, 255, newColor.G, newColor.B), 0);
		SliderG.Background = new LinearGradientBrush(Color.FromArgb(alpha, newColor.R, 0, newColor.B), Color.FromArgb(alpha, newColor.R, 255, newColor.B), 0);
		SliderB.Background = new LinearGradientBrush(Color.FromArgb(alpha, newColor.R, newColor.G, 0), Color.FromArgb(alpha, newColor.R, newColor.G, 255), 0);
		SliderA.Background = new LinearGradientBrush(Color.FromArgb(0, newColor.R, newColor.G, newColor.B), Color.FromArgb(255, newColor.R, newColor.G, newColor.B), 0);
	}

	private void Border_PreviewKeyDown(object sender, KeyEventArgs e)
	{
		bool controlPressed = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;

		if (e.Key == Key.C && controlPressed)
		{
			WinApi.CopyTextToClipboard(ColorHex.Text);

			e.Handled = true;
			return;
		}
		else if (e.Key == Key.V && controlPressed)
		{
			string colorString = Clipboard.GetText();

			SolidColorBrush newBrush = colorString.ToBrush();
			if (newBrush != null)
			{
				SliderR.Value = newBrush.Color.R;
				SliderG.Value = newBrush.Color.G;
				SliderB.Value = newBrush.Color.B;
				SliderA.Value = newBrush.Color.A;

				e.Handled = true;
				return;
			}
		}
		e.Handled = false;
	}

	private void ColorHex_TextChanged(object sender, TextChangedEventArgs e)
	{
		if (selectedRectangle != null)
		{
			if (ColorHex.Text.ToBrush() != null)
			{
				ColorHex.Error = false;
				selectedRectangle.Fill = ColorHex.Text.ToBrush();
			}
			else
			{
				ColorHex.Error = true;
			}
		}
	}

	#endregion

}
