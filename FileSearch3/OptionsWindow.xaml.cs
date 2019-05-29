using Microsoft.Win32;
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
			Rectangle rectangle = e.Source as Rectangle;

			ColorDialog colorDialog = new ColorDialog();

			if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				Color newColor = Color.FromArgb((byte)(sender == SelectionBackground ? 50 : 255), colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
				rectangle.Fill = new SolidColorBrush(newColor);
			}
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
			DialogResult = true;
		}

		#endregion

	}
}
