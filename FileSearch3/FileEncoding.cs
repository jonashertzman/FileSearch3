﻿using System.Text;

namespace FileSearch;

public class FileEncoding(Encoding type, bool bom, NewlineMode newline, bool endOfFileNewline)
{

	#region Overrides

	public override string ToString()
	{
		string name = "";

		if (Type == Encoding.UTF8)
		{
			name = "UTF-8";
		}
		else if (Type == Encoding.Unicode)
		{
			name = "UTF-16 LE";
		}
		else if (Type == Encoding.BigEndianUnicode)
		{
			name = "UTF-16 BE";
		}
		else if (Type == Encoding.UTF32)
		{
			name = "UTF-32";
		}
		else if (Type == Encoding.Default)
		{
			name = Type.WebName;
		}

		if (Bom)
		{
			name += " BOM";
		}

		name += $" ({Newline})";

		return name;
	}

	#endregion

	#region Properties

	public Encoding Type { get; private set; } = type;

	public bool Bom { get; private set; } = bom;

	public NewlineMode Newline { get; private set; } = newline;

	public bool EndOfFileNewline { get; private set; } = endOfFileNewline;

	public Encoding GetEncoding
	{
		get
		{
			if (Type == Encoding.UTF8)
			{
				return new UTF8Encoding(Bom);
			}
			else if (Type == Encoding.Unicode)
			{
				return new UnicodeEncoding(false, Bom);
			}
			else if (Type == Encoding.BigEndianUnicode)
			{
				return new UnicodeEncoding(true, Bom);
			}
			else if (Type == Encoding.UTF32)
			{
				return new UTF32Encoding(false, Bom);
			}

			return Encoding.Default;
		}
	}

	public string GetNewLineString
	{
		get
		{
			if (Newline == NewlineMode.Mac)
			{
				return "\r";
			}
			else if (Newline == NewlineMode.Unix)
			{
				return "\n";
			}
			return "\r\n";
		}
	}

	#endregion

}
