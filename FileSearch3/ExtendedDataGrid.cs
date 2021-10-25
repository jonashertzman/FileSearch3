using System.Windows.Controls;
using System.Windows.Input;

namespace FileSearch;

class ExtendedDataGrid : DataGrid
{

	#region Overrides

	protected override void OnKeyDown(KeyEventArgs e)
	{
		if (e.Key == Key.Left || e.Key == Key.Right)
		{
			return;
		}

		base.OnKeyDown(e);
	}

	#endregion

}
