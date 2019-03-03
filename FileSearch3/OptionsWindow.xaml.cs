using System.Globalization;
using System.Linq;
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

		#region Constructor

		public OptionsWindow()
		{
			InitializeComponent();

			foreach (FontFamily family in Fonts.SystemFontFamilies.OrderBy(x => x.Source))
			{
				ComboBoxFont.Items.Add(family.Source);
			}
		}

		#endregion

		#region Events

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
				rectangle.Fill = new SolidColorBrush(Color.FromRgb(colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B));
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
