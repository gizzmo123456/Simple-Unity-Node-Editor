using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Obsolete( "It has no functions any more! Use BaseNodeGraphEditor and use 'BaseDialogueGraphVertPinData' as it minimal T type" )]
public abstract class BaseNodeGraphVertPinsEditor<T> : BaseNodeGraphEditor<T> where T : BaseGraphVertPinData
{

	public BaseNodeGraphVertPinsEditor ( int uid ) : base( uid ) { }
	public BaseNodeGraphVertPinsEditor ( int uid, Rect pannelRect ) : base( uid, pannelRect ) { }

}

public abstract class BaseGraphVertPinData : BaseNodeGraphData
{

	public BaseGraphVertPinData ( string _title, bool _dragable ) : base( "", _dragable ) { }
	public BaseGraphVertPinData ( string _title, bool _dragable, int _max_inputPins, int _max_outputPins ) : 
		base( "", _dragable, _max_inputPins, _max_outputPins ) { }

	public BaseGraphVertPinData ( string _title, bool _dragable, Vector2 _inputStartPosition, Vector2 _outputStartPosition, Vector2 _pinSize ) :
		base( "", _dragable, _inputStartPosition, _outputStartPosition, _pinSize )
	{ }


	public override NodePin_Output.BezierControlePointOffset BezierControlePointOffset { get => NodePin_Output.BezierControlePointOffset.Vertical; }

	protected override void GenerateNodeSize ()
	{
		rect.size = new Vector2( 300, 150 );
	}

	public override void GeneratePinSizeAndPosition ()
	{

		GenerateNodeSize();

		output_pinSize.x = ( rect.width / NodeConnections_output.Count ) - (25f / NodeConnections_output.Count );

		inputPin_localStartPosition.x = -output_pinSize.x + 12;
		inputPin_localStartPosition.y = 0;

		outputPin_localStartPosition.x = -output_pinSize.x + 12;
		outputPin_localStartPosition.y = rect.height - ( output_pinSize.y * 2f );

	}

	protected override Rect GetNodeContentsPosition ()
	{
		Vector2 nodeSize = rect.size;

		return new Rect()
		{
			y = 0,
			x = 22,
			width = nodeSize.x - 10,
			height = nodeSize.y - 44
		};

	}

	protected override Vector2 GetPinOffset ( int pinId, PinMode pinMode )
	{
		Vector2 pinOffset = Vector2.zero;

		switch ( pinMode )
		{
			case PinMode.Input:
			pinOffset.x = output_pinSize.x;
			break;
			case PinMode.Output:
			pinOffset = output_pinSize;
			pinOffset.x += ( output_pinSize.x ) * pinId;
			break;
		}

		return pinOffset;

	}

	public override Vector2 GetConnectionOffset ( PinMode pinMode )
	{
		switch ( pinMode )
		{
			case PinMode.Input:
			return new Vector2( output_pinSize.x / 2f, 0 );
			case PinMode.Output:
			return new Vector2( output_pinSize.x / 2f, output_pinSize.y );
		}

		return Vector2.zero;
	}

}
