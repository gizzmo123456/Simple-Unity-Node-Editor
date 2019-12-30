using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BaseNodeGraphEditor : BaseNodeEditor<BaseNodeGraphData>
{
	public BaseNodeGraphEditor (int uid) : base(uid) { }
	public BaseNodeGraphEditor (int uid, Rect pannelRect) : base(uid, pannelRect) { }

	public override void Draw ( EditorWindow window )
	{
		base.Draw( window );

		DrawNodeConnections();

	}

	protected override Vector2 NodeSize ()
	{
		return new Vector2( 75, 50 );
	}

	public override BaseNodeGraphData AddNode ( string title, bool isDragable )
	{
		BaseNodeGraphData data = new BaseNodeGraphData()
		{
			dragable = isDragable,
			title = title
		};

		return AddNode( data );

	}

	public override BaseNodeGraphData AddNode ( BaseNodeGraphData data )	// TODO: move into baseNodeEditor.
	{
		data.SetNodePosition( NodeStartPosition() );
		data.SetNodeSize( NodeSize() );

		nodes.Add( data );

		return data;
	}

	protected override Rect ClampNodePosition ( Rect nodeRect, int winId = 0 )	// TODO: should be default?
	{

		if ( nodeRect.x < panelRect.x - panelScrollPosition.x ) nodeRect.x = panelRect.x - panelScrollPosition.x;
		if ( nodeRect.y < panelRect.y - panelScrollPosition.y ) nodeRect.y = panelRect.y - panelScrollPosition.y;

		return nodeRect;
	}

	protected override void NodeWindow ( int windowId )
	{
		base.NodeWindow( windowId );

		int node_id = windowId - uniqueID;

		DrawNodePins( node_id, true );

	}

	protected virtual void DrawNodePins(int node_id, bool output)
	{

		// output pins are drawn on the lhs of the node.
		// inputs pins are drawn on the rhs of the node.

		NodeConnection_Input[] nodeConnectionPins = nodes[ node_id ].NodeConnections_outputs.ToArray(); // connections output inherit from input and we only need the data that inputs class have.

		// TODO: these need to be set in the class...
		Vector2 pinPosition = new Vector2( NodeSize().x - ( NodeSize().x / 4f ) , 20 );
		Vector2 pinSize = new Vector2(NodeSize().x / 4f, 20);

		for (int i = 0; i < nodeConnectionPins.Length; i++ )
		{
			string lable = nodeConnectionPins[ i ].connectionLable;

			if ( output ) lable += "*" + lable;
			else lable = "*" + lable;

			GUI.Label( new Rect( pinPosition, pinSize ), lable );
			pinPosition.y += pinSize.y;
		}


	}


	////////// Node Connections
	/* Node outputs connect to node inputs, the connections are draw by the outputing node.
	 * 
	 * 
	 */
	protected virtual void DrawNodeConnections()
	{

		for( int i = 0; i < nodes.Count; i++ )
		{
			
			foreach ( NodeConnection_Output nodeConn in nodes[i].NodeConnections_outputs )
			{
				Vector2 startPos = GetConnectionPosition( i, true, nodeConn );
				Vector2 endPos = GetConnectionPosition( nodeConn.connectedNodeId, false, nodeConn );

				if ( !PositionIsVisable( startPos ) && !PositionIsVisable( endPos ) ) continue; // do not draw connect if both start and end points are not visable

				nodeConn.DrawConnection(startPos, endPos, PositionIsVisable);
			}
		}

	}

	protected virtual Vector2 GetConnectionPosition(int nodeId, bool output, NodeConnection_Output connectionData )
	{
		// return nodes[ nodeId ].NodeRect.center + GetNodeOffset();  // default when abstract

		Rect nodeRect = nodes[ nodeId ].NodeRect;

		if (output)
		{
			return new Vector2( nodeRect.x + nodeRect.width, nodeRect.center.y );
		}
		else
		{
			return new Vector2( nodeRect.x, nodeRect.center.y );
		}
	}

}

public class BaseNodeGraphData : BaseNodeData
{
	List<NodeConnection_Output> nodeConnections_outputs = new List<NodeConnection_Output>();
	public List<NodeConnection_Output> NodeConnections_outputs { get => nodeConnections_outputs; }

	public NodeConnection_Output AddConnection (int connectToNodeId, string connectionLable)
	{
		nodeConnections_outputs.Add( new NodeConnection_Output( nodeConnections_outputs.Count, connectionLable, connectToNodeId ) );

		return nodeConnections_outputs[ nodeConnections_outputs.Count - 1 ];

	}

	public void RemoveConnection( int connectionId )
	{
		nodeConnections_outputs.RemoveAt( connectionId );
	}

	public void RemoveConnection( NodeConnection_Output nodeConn )
	{
		nodeConnections_outputs.Remove( nodeConn );
	}
}

public class NodeConnection_Input
{
	public int id;
	public string connectionLable;

	public NodeConnection_Input(int _id, string connLable)
	{
		id = _id;
		connectionLable = connLable;
	}

}

public class NodeConnection_Output : NodeConnection_Input
{
	public delegate bool isVisableFunct ( Vector2 position );
	
	public int connectedNodeId;		// the node id to connect to.
	public int connectedSlotId;		// the input slot that the node is connected to.

	public bool alwaysForwardControlPoints = true;

	public Color curveColor = Color.black;

	// Catch the start and end positions so we only update the curve when they change
	Vector2 connectionStartPosition = Vector2.zero;
	Vector2 connectionEndPosition = Vector2.one;

	const int curvePoints = 20;
	Vector2[] connectionCurve = new Vector2[ curvePoints + 1 ];

	public NodeConnection_Output(int _id, string connLable, int connNodeId) : base(_id, connLable)
	{
		connectedNodeId = connNodeId;
	}

	public void DrawConnection(Vector2 startPosition, Vector2 endPosition, isVisableFunct isVisable )
	{
		if ( startPosition != connectionStartPosition || endPosition != connectionEndPosition )
		{
			connectionStartPosition = startPosition;
			connectionEndPosition = endPosition;
			GenerateBezierCurve();
		}

		Handles.color = curveColor;
		
		for ( int i = 1; i < curvePoints + 1; i++ )
		{
			Vector2 lineCenter = connectionCurve[ i - 1 ] + ( ( connectionCurve[ i ] - connectionCurve[ i - 1 ] ) / 2f );
			if (isVisable( lineCenter ) )
				Handles.DrawLine( connectionCurve[ i-1 ], connectionCurve[ i ] );
		}

	}

	public void GenerateBezierCurve()
	{
		float xOffset = ( connectionEndPosition.x - connectionStartPosition.x ) * (0.75f + (0.1f * (Mathf.Abs( connectionEndPosition.y - connectionStartPosition.y ) / 150f) )) ;

		if ( alwaysForwardControlPoints )
			xOffset = Mathf.Abs( xOffset );

		Vector2 startControlPoint = connectionStartPosition + new Vector2( xOffset, 0 );
		Vector2 endControlPoint = connectionEndPosition + new Vector2( -xOffset, 0 );

		for ( int i = 0; i < curvePoints + 1; i++ ) 
		{
			float t = (float)i / (float)curvePoints;
			connectionCurve[ i ] = ( Mathf.Pow( 1f - t, 3 ) * connectionStartPosition ) +
								   ( 3f * Mathf.Pow( 1f - t, 2 ) * t * startControlPoint ) + 
								   ( 3f * ( 1 - t ) * Mathf.Pow( t, 2 ) * endControlPoint ) + 
								   ( Mathf.Pow( t, 3 ) * connectionEndPosition );

		}
	}

}