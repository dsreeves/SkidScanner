using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SkidScanner.Models
{
	public class SkidLst 
	{
		public List<String> Barcodes = new List<string>();
		public string _barcode { get; set; }
		public string _tag { get; set; }
		public string _dateTime { get; set; }
		public bool _iniSuccess = false;

		public SkidLst(string tag)
		{
			_tag = Regex.Replace(tag, @"[-]", "");
			_iniSuccess = SQLite.CreateTable(_tag);
		}

		public bool Add(string str)
		{
			if (Check(str))
			{
				Barcodes.Add(str);
				_barcode = str;
				_dateTime = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
				SQLite.WriteBarcode(this);
				return true;
			}
			else
			{
				return false;
			}
		}

		private bool Check(string str)
		{
			if (Barcodes.Contains(str))
			{
				return false;
			}
			else
			{
				return true;
			}
		}

	}
}
