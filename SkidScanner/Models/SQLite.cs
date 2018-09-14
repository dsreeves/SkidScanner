using Dapper;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SkidScanner.Models
{
	public class SQLite
	{
		private static string LoadConnectionString(string id = "Default")
		{
			return ConfigurationManager.ConnectionStrings[id].ConnectionString;
		}

		public static bool CreateTable(string tag)
		{
			SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString());
			cnn.Open();
			StringBuilder sb = new StringBuilder();
			var s1 = "CREATE TABLE ";
			var s2 = " (dt string, serial string)";

			var s4 = sb.Append(s1 + tag + s2).ToString();
			
			try
			{
				SQLiteCommand cmd = new SQLiteCommand(s4, cnn);
				cmd.ExecuteNonQuery();
			}
			catch (SQLiteException e)
			{
				cnn.Close();
				return false;
			}
			cnn.Close();
			return true;
		}

		public static bool WriteBarcode(SkidLst v)
		{
			using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
			{
				try
				{
					cnn.Execute($"INSERT INTO {v._tag} (dt, serial) values (@_dateTime, @_barcode)", v);
				}
				catch (SQLiteException e)
				{
					if (e.ErrorCode.ToString() == "19")
					{
						return false;
					}
				}
				return true;

			}
		}

	}
}
