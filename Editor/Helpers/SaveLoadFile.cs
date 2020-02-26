using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Helpers
{
	[System.Serializable]
	public class SaveLoadFile
	{

		public string path;
		public string fileName;
		public string fileExtension;
		string FullFileName => string.Format( "{0}{1}.{2}", path, fileName, fileExtension );

		public bool createIfDoesNotExist = true;

		List<string> file_lines = new List<string>();
		public string AllLines => string.Join( "\n", file_lines );

		public int LineCount { get { return file_lines.Count; } }

		public SaveLoadFile( string path_, string _fileName, string _exe, bool _createNotExist )
		{
			path = path_;
			fileName = _fileName;
			fileExtension = _exe;
			createIfDoesNotExist = _createNotExist;
		}

		public string[] GetFile()
		{
			return file_lines.ToArray();
		}

		public string GetLine( int lineId )
		{
			return file_lines[ lineId ];
		}

		public void AddLine( string str )
		{
			file_lines.Add( str );
		}

		public void AddLines( string[] strs)
		{
			file_lines.AddRange( strs );
		}

		public void ReplaceLine(int line_id, string str)
		{
			if ( line_id < file_lines.Count )
				file_lines[ line_id ] = str;
			else
				file_lines.Add(str);
		}

		public void ReplaceFile( string file )
		{
			file_lines.Clear();
			file_lines.AddRange( file.Split( '\n' ) );
		}

		public void RemoveLine( int line_id )
		{
			file_lines.RemoveAt( line_id );
		}

		public void LoadFile()
		{
			if ( FileExists( path, fileName, fileExtension ) )
			{
				if ( FileInUse( path, fileName, fileExtension ) )
				{
					Debug.Log( "Can not read file, File in use" );
					return;
				}

				file_lines.Clear();
				file_lines.AddRange( System.IO.File.ReadAllLines( FullFileName ));
			}
		}

		public void SaveFile()
		{
			//Check that the path exists if not create it
			PathExists( path, true );
			System.IO.File.WriteAllLines( FullFileName, file_lines );
			Debug.Log( "File Saved '" + FullFileName +"' @ " + Time.realtimeSinceStartup );

		}

		public static bool FileExists(string p, string fn, string exe)
		{
			string fullFileName = string.Format( "{0}{1}.{2}", p, fn, exe );

			if ( !System.IO.File.Exists( fullFileName ) )
			{
				Debug.Log( "File Does not exists ("+ fullFileName +")" );
				return false;
			}

			return true;
		}

		public static bool PathExists( string p, bool createNew = false )
		{

			if ( !System.IO.Directory.Exists( p ) )
			{
				if ( createNew )
				{
					System.IO.DirectoryInfo di = System.IO.Directory.CreateDirectory( p );
					Debug.Log( "Path Does not Exists: " + ( di.Exists ? "Creating new Path" : "Failed to creat new path" ) + " (" + p + ")" );
					return di.Exists;
				}
				else
				{
					Debug.Log( "Path Does not Exists (" + p + ")" );
					return false;
				}
			}
			else
			{

				return true;

			}
		}

		public static bool FileInUse( string path, string file, string exe )
		{

			string fullFileName = string.Format( "{0}{1}.{2}", path, file, exe );

			try
			{
				System.IO.FileStream fs = System.IO.File.Open( fullFileName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.None );
				fs.Close();
			}
			catch ( System.IO.IOException )
			{
				return true;
			}

			return false;

		}

	}

}
