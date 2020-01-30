using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Obsolete]
public class ExampleGraphVertPinEditor : BaseNodeGraphVertPinsEditor<ExampleGraphVertPinData>
{
	public ExampleGraphVertPinEditor ( int uid ) : base( uid ) { }
	public ExampleGraphVertPinEditor ( int uid, Rect pannelRect ) : base( uid, pannelRect ) { }

}

public class ExampleGraphVertPinData : BaseGraphVertPinData
{
	public string exampleText = "Helloooo World";

	public ExampleGraphVertPinData ( string _title, bool _dragable, string text ) : base( _title, _dragable )
	{
		exampleText = text;
	}

	/// <param name="_inputStartPosition"> Local start position of input pins </param>
	/// <param name="_outputStartPosition"> Local start position of output pind </param>
	/// <param name="pinSize">size of pin</param>
	public ExampleGraphVertPinData ( string _title, bool _dragable, Vector2 _inputStartPosition, Vector2 _outputStartPosition, Vector2 _pinSize, string text )
		: base( _title, _dragable, _inputStartPosition, _outputStartPosition, _pinSize )
	{
		exampleText = text;
	}

	protected override void NodeUi ()
	{
		GUI.Label( new Rect( 0, 0, 300, 300 ), exampleText );
	}
}