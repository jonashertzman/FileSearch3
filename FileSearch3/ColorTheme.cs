using System.Reflection;

namespace FileSearch;

public class ColorTheme
{

	#region Properties

	public required string Name { get; set; }

	// Editor colors
	public required string NormalForeground { get; set; }
	public required string NormalBackground { get; set; }

	public required string HitForeground { get; set; }
	public required string HitBackground { get; set; }

	public required string HeaderForeground { get; set; }
	public required string HeaderBackground { get; set; }

	public required string LineNumberColor { get; set; }
	public required string CurrentDiffColor { get; set; }
	public required string SelectionBackground { get; set; }

	// UI colors
	public required string NormalText { get; set; }
	public required string DisabledText { get; set; }
	public required string DisabledBackground { get; set; }

	public required string WindowBackground { get; set; }
	public required string DialogBackground { get; set; }

	public required string ControlLightBackground { get; set; }
	public required string ControlDarkBackground { get; set; }

	public required string BorderLight { get; set; }
	public required string BorderDark { get; set; }

	public required string HighlightBackground { get; set; }
	public required string HighlightBorder { get; set; }

	public required string AttentionBackground { get; set; }

	#endregion

	#region Overrides 

	public override string ToString()
	{
		return Name;
	}

	#endregion

	#region Methods

	public ColorTheme Clone()
	{
		return (ColorTheme)MemberwiseClone();
	}

	internal void SetDefaultsIfNull(ColorTheme defaultTheme)
	{
		foreach (PropertyInfo propertyInfo in this.GetType().GetProperties())
		{
			if (propertyInfo.GetValue(this) == null)
			{
				propertyInfo.SetValue(this, propertyInfo.GetValue(defaultTheme));
			}
		}
	}

	internal void SetDefaults(ColorTheme defaultTheme)
	{
		foreach (PropertyInfo propertyInfo in this.GetType().GetProperties())
		{
			propertyInfo.SetValue(this, propertyInfo.GetValue(defaultTheme));
		}
	}

	#endregion

}
