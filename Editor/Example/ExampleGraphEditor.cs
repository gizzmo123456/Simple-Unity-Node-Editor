using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Obsolete]
public class ExampleGraphEditor : BaseNodeGraphEditor<ExampleNodeGraphData>
{

	public ExampleGraphEditor ( int uid ) : base( uid ) { }
	public ExampleGraphEditor ( int uid, Rect pannelRect ) : base( uid, pannelRect ){}

}

public class ExampleNodeGraphData : BaseNodeGraphData
{
	public string exampleText = "Helloooo World";

	public ExampleNodeGraphData ( string _title, bool _dragable, string text ) : base( _title, _dragable )
	{
		exampleText = text;
	}

	/// <param name="_inputStartPosition"> Local start position of input pins </param>
	/// <param name="_outputStartPosition"> Local start position of output pind </param>
	/// <param name="pinSize">size of pin</param>
	public ExampleNodeGraphData ( string _title, bool _dragable, Vector2 _inputStartPosition, Vector2 _outputStartPosition, Vector2 _pinSize, string text) 
		: base( _title, _dragable, _inputStartPosition, _outputStartPosition, _pinSize ) 
	{ 
		exampleText = text; 
	}

	protected override void NodeUi ()
	{
		GUI.Label( new Rect( 0, 0, 300, 300 ), exampleText );
	}
}
