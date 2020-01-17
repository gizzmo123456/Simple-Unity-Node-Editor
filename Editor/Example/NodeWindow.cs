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

	ExampleVerticalEditor nodeWindow;
	ExampleGraphEditor nodeGraphEditor;
	ExampleGraphVertPinEditor nodeGrapthVertEditor;

	private void Awake ()
	{
		
		nodeWindow = new ExampleVerticalEditor(0, new Rect(100, 0, 500, 150));
		nodeWindow.AddNode( new ExampleVerticalNodeData(true, "Win 1") );
		nodeWindow.AddNode( new ExampleVerticalNodeData(true, "Win 2") );
		nodeWindow.AddNode( new ExampleVerticalNodeData(true, "Win 3") );
		nodeWindow.AddNode( new ExampleVerticalNodeData(true, "Win 4") );
		nodeWindow.AddNode( new ExampleVerticalNodeData(true, "Win 5") );
		nodeWindow.AddNode( new ExampleVerticalNodeData(true, "Win 6") );
		nodeWindow.AddNode( new ExampleVerticalNodeData(true, "Win 7") );
		nodeWindow.AddNode( new ExampleVerticalNodeData(true, "Win 8") );
		nodeWindow.AddNode( new ExampleVerticalNodeData(true, "Win 9") );
		nodeWindow.AddNode( new ExampleVerticalNodeData(true, "Win 10") );
		nodeWindow.AddNode( new ExampleVerticalNodeData(true, "Win 11") );
		nodeWindow.AddNode( new ExampleVerticalNodeData(true, "Win 12") );

		nodeWindow.NodeMoved += VertNodeMoved;	// Get notifed when the order changes

		nodeGraphEditor = new ExampleGraphEditor( 1, new Rect(50, 175, 900, 350));
		nodeGraphEditor.AddNode( new ExampleNodeGraphData("Node 0", true, "Helloo World\n000") );	// Node 0
		nodeGraphEditor.AddNode( new ExampleNodeGraphData("Node 1", true, "Helloo World\n001") );	// Node 1
		nodeGraphEditor.AddNode( new ExampleNodeGraphData("Node 2", true, "Helloo World\n002") );	// Node 2
		nodeGraphEditor.AddNode( new ExampleNodeGraphData("Node 3", true, "Helloo World\n003") );	// Node 3
		nodeGraphEditor.AddNode( new ExampleNodeGraphData("Node 4", true, "Helloo World\n004") );	// Node 4


		nodeGraphEditor.AddPin_toNode( 0, "OUT 1", BaseNodeGraphData.PinMode.Output );  // Node 0, pin 0, output
		nodeGraphEditor.GetNode( 0 ).SetOutputPinColor ( 0, new Color(1f, 0.1f, 0.1f) );// Chnage the color of connection node 0, pin 0
		
		nodeGraphEditor.AddOutputPin_toNode( 0, "OUT 2", new Color(0.1f, 0.1f, 1f) );   // Node 0, pin 1, output // add output pin of color
		nodeGraphEditor.AddPin_toNode( 0, "OUT 3", BaseNodeGraphData.PinMode.Output );  // Node 0, pin 2, output
		nodeGraphEditor.AddPin_toNode( 0, "OUT 4", BaseNodeGraphData.PinMode.Output );  // Node 0, pin 3, output
		nodeGraphEditor.AddPin_toNode( 0, "IN 1", BaseNodeGraphData.PinMode.Input );	// Node 0, pin 0, input

		nodeGraphEditor.AddPin_toNode( 1, "OUT 1", BaseNodeGraphData.PinMode.Output );	// Node 1, pin 0, output
		nodeGraphEditor.AddPin_toNode( 1, "IN 1", BaseNodeGraphData.PinMode.Input );    // Node 1, pin 0, input
		nodeGraphEditor.AddPin_toNode( 1, "IN 2", BaseNodeGraphData.PinMode.Input );	// Node 1, pin 1, input
		nodeGraphEditor.AddPin_toNode( 1, "IN 3", BaseNodeGraphData.PinMode.Input );	// Node 1, pin 2, input

		nodeGraphEditor.AddPin_toNode( 2, "IN 1", BaseNodeGraphData.PinMode.Input );

		nodeGraphEditor.AddPin_toNode( 3, "OUT 4", BaseNodeGraphData.PinMode.Output );
		nodeGraphEditor.AddPin_toNode( 3, "OUT 4", BaseNodeGraphData.PinMode.Output );


		nodeGraphEditor.GetNode( 0 ).AddConnection( 0, 1, 0 );		// connect (output) node 0, pin 0 to (input) node 1, pin 0
		nodeGraphEditor.GetNode( 0 ).AddConnection( 1, 1, 2 );		// connect (output) node 0, pin 1 to (input) node 1, pin 2
		//nodeGraphEditor.GetNode( 0 ).AddConnection( 2, 2, 0 ); 	// connect (output) node 0, pin 2 to (input) node 2, pin 0
		nodeGraphEditor.GetNode( 0 ).AddConnection( 0, 2, 0 );      // connect (output) node 0, pin 0 to (input) node 2, pin 0
		nodeGraphEditor.GetNode( 0 ).AddConnection( 0, 2, 0 );      // connect (output) node 0, pin 0 to (input) node 2, pin 0
		nodeGraphEditor.GetNode( 0 ).AddConnection( 0, 2, 0 );      // connect (output) node 0, pin 0 to (input) node 2, pin 0
																	//nodeGraphEditor.GetNode( 1 ).AddConnection( 0, 2, 0 ); 	// connect (output) node 1, pin 0 to (input) node 2, pin 0

		nodeGraphEditor.NodeConnection += NodeConnected; // Get noifed when a node is connected.

		nodeGrapthVertEditor = new ExampleGraphVertPinEditor( 2, new Rect( 50, 575, 900, 350 ) );
		nodeGrapthVertEditor.AddNode( new ExampleGraphVertPinData( "Node 0", true, "Helloo World\n000" ) ); // Node 0
		nodeGrapthVertEditor.AddNode( new ExampleGraphVertPinData( "Node 1", true, "Helloo World\n001" ) ); // Node 1
		nodeGrapthVertEditor.AddNode( new ExampleGraphVertPinData( "Node 2", true, "Helloo World\n001" ) ); // Node 1

		nodeGrapthVertEditor.AddPin_toNode( 0, "OUT 1", BaseNodeGraphData.PinMode.Output );  // Node 0, pin 0, output
		nodeGrapthVertEditor.AddPin_toNode( 0, "OUT 1", BaseNodeGraphData.PinMode.Output );  // Node 0, pin 1, output

		nodeGrapthVertEditor.AddPin_toNode( 1, "In 1", BaseNodeGraphData.PinMode.Input );  // Node 0, pin 0, output
		nodeGrapthVertEditor.AddPin_toNode( 2, "In 1", BaseNodeGraphData.PinMode.Input );  // Node 0, pin 0, output

		nodeGrapthVertEditor.GetNode( 0 ).AddConnection( 0, 1, 0 );      // connect (output) node 0, pin 0 to (input) node 1, pin 0
		nodeGrapthVertEditor.GetNode( 0 ).AddConnection( 1, 2, 0 );      // connect (output) node 0, pin 0 to (input) node 1, pin 0

	}

	private void OnGUI ()
	{
		BeginWindows();

		nodeWindow.Draw(this);
		nodeGraphEditor.Draw(this);
		nodeGrapthVertEditor.Draw( this );
		EndWindows();

	}

	void VertNodeMoved ( int fromId, int toId)
	{
		Debug.Log( string.Format( "Node moved from {0}, to {1} ", fromId, toId) );

	}

	void NodeConnected( ExampleGraphEditor.ConnectNodesStatus connectionType, int fromNodeId, int fromSlotId, int toNodeId, int toSlotId )
	{
		Debug.Log( string.Format( "Connection status: {0}, From node: {1} slot {2}, To node {3} slot {4}", connectionType, fromNodeId, fromSlotId, toNodeId, toSlotId ) );
	}

}
