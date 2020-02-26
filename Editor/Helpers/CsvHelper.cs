using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Helpers
{
	[System.Serializable]
	public class CSV
	{

		SaveLoadFile csvFile;
		public List<csv_row> rows = new List<csv_row>();

		public CSV( string path, string fileName )
		{
			csvFile = new SaveLoadFile( path, fileName, "csv", true );
		}

		/// <summary>
		/// Add a new row to the csv
		/// </summary>
		public void AddRow( string[] values)
		{
			rows.Add( new csv_row( values ) );
		}

		/// <summary>
		///  apends to the end of row id
		/// </summary>
		public void AddToRow( int rowId, string[] values)
		{
			rows[ rowId ].AddValues( values );
		}

		/// <summary>
		/// removes row at rowid
		/// </summary>
		public void RemoveRow( int rowId)
		{
			rows.RemoveAt( rowId );
		}


		/// <summary>
		/// replaces row at row id
		/// </summary>
		public void ReplaceRow( int rowId, string[] values)
		{
			if ( rowId < rows.Count )
				rows[ rowId ] = new csv_row( values );
			else
				rows.Add( new csv_row( values ) );
		}

		public string[] GetRow( int rowId )
		{
			return rows[ rowId ].values.ToArray();
		}

		/// <summary>
		/// returns row in CSV format
		/// </summary>
		public string GetRowString( int rowId)
		{

			return rows[ rowId ].ToString();

		}

		/// <summary>
		/// returns all rows in csv format
		/// </summary>
		/// <returns></returns>
		public string GetAllRows()
		{

			string str = "";

			for( int i = 0; i < rows.Count; i++ )
			{

				str += rows[ i ].ToString();

				if ( i < rows.Count - 1 )
					str += "\n";

			}

			return str;

		}

		public void SaveCSV()
		{

			
			for ( int i = 0; i < rows.Count; i++ )
				csvFile.ReplaceLine(i, GetRowString(i) );

			csvFile.SaveFile();

		}

		public void LoadCSV()
		{

			csvFile.LoadFile();

			for ( int i = 0; i < csvFile.LineCount; i++ )
				ReplaceRow( i, csvFile.GetLine( i ).Split( ',' ) );

		}

		public class csv_row
		{
			public List<string> values = new List<string>();

			public csv_row( string[] vals )
			{
				values.AddRange(vals);
			}

			public void AddValues( string[] vals )
			{
				values.AddRange( vals );
			}

			public override string ToString()
			{

				string str = "";

				for( int i = 0; i < values.Count; i++ )
				{

					//string.
					str += values[ i ];

					if ( i < values.Count - 1 )
						str += ",";

				}

				return str;

			}

		}

	}

}