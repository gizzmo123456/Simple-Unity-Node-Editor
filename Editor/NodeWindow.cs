using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NodeWindow : EditorWindow
{

	[MenuItem( "Nodes/NodeEditor" )]
	public static void ShowWindow ()
	{
		GetWindow( typeof( NodeWindow ) );
	}

	BaseVerticalEditor nodeWindow;
	BaseNodeGraphEditor nodeGraphEditor;

	private void Awake ()
	{

		nodeWindow = new BaseVerticalEditor(0, new Rect(100, 100, 500, 200));
		nodeWindow.AddNode("Win 1", true);
		nodeWindow.AddNode("Win 2", true);
		nodeWindow.AddNode("Win 3", true);
		nodeWindow.AddNode("Win 4", true);
		nodeWindow.AddNode("Win 5", true);
		nodeWindow.AddNode("Win 6", true);
		nodeWindow.AddNode("Win 7", true);
		nodeWindow.AddNode("Win 8", true);
		nodeWindow.AddNode("Win 9", true);
		nodeWindow.AddNode("Win 10", true);

		nodeGraphEditor = new BaseNodeGraphEditor(1, new Rect(50, 400, 900, 500));
		nodeGraphEditor.AddNode( "Node 1", true );
		nodeGraphEditor.AddNode( "Node 2", true );
		nodeGraphEditor.AddNode( "Node 3", true );

		nodeGraphEditor.AddPin_toNode( 0, "OUT 1", BaseNodeGraphData.PinMode.Output );
		nodeGraphEditor.AddPin_toNode( 0, "OUT 2", BaseNodeGraphData.PinMode.Output );
		nodeGraphEditor.AddPin_toNode( 0, "OUT 3", BaseNodeGraphData.PinMode.Output );
		nodeGraphEditor.AddPin_toNode( 0, "IN 1", BaseNodeGraphData.PinMode.Input );

		nodeGraphEditor.AddPin_toNode( 1, "OUT 1", BaseNodeGraphData.PinMode.Output );
		nodeGraphEditor.AddPin_toNode( 1, "IN 1", BaseNodeGraphData.PinMode.Input );
		nodeGraphEditor.AddPin_toNode( 1, "IN 2", BaseNodeGraphData.PinMode.Input );
		nodeGraphEditor.AddPin_toNode( 1, "IN 3", BaseNodeGraphData.PinMode.Input );

		nodeGraphEditor.AddPin_toNode( 2, "IN 1", BaseNodeGraphData.PinMode.Input );


		nodeGraphEditor.GetNode( 0 ).AddConnection( 0, 1, 0 );
		nodeGraphEditor.GetNode( 0 ).AddConnection( 1, 1, 2 );
		nodeGraphEditor.GetNode( 0 ).AddConnection( 2, 2, 0 ); 
		nodeGraphEditor.GetNode( 0 ).AddConnection( 0, 2, 0 ); 
		nodeGraphEditor.GetNode( 1 ).AddConnection( 0, 2, 0 ); 
		//nodeGraphEditor.GetNode( 0 ).AddConnection( 2, "Out Pin 3" );
		//nodeGraphEditor.GetNode( 1 ).AddConnection( 2, "Out Pin 1" );
		//nodeGraphEditor.GetNode( 2 ).AddConnection( 0, "Out Pin 2" );

	}

	private void Update ()
	{
		//nodeWindow.Update();
		
	}

	private void OnGUI ()
	{
		BeginWindows();

		nodeWindow.Draw(this);
		nodeGraphEditor.Draw(this);
		
		EndWindows();
/*
		Vector3[] v = { new Vector3(0, 100, 0),       new Vector3( 100, 100, 0 ),
						new Vector3(100, 200, 0), new Vector3(0, 200, 0) };
		Handles.DrawSolidRectangleWithOutline( v, Color.black, Color.red );
*/
	}



}
