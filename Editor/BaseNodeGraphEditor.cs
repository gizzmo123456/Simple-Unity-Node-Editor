using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BaseNodeGraphEditor : BaseNodeEditor<BaseNodeGraphData>
{
	public BaseNodeGraphEditor () : base() { }
	public BaseNodeGraphEditor (Rect pannelRect) : base(pannelRect) { }

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
		return nodeRect;
	}

	protected override void NodeWindow ( int windowId )
	{
		base.NodeWindow( windowId );
	}

////////// Node Connections
	protected virtual void DrawNodeConnections()
	{

		foreach( BaseNodeGraphData node in nodes )
		{
			Vector2 startPos = node.GetNodePosition();
			foreach ( NodeConnection nodeConn in node.NodeConnections )
			{
				Vector2 endPos = nodes[ nodeConn.connectedNodeId ].GetNodePosition();
				nodeConn.DrawConnection(startPos, endPos);
			}
		}

	}

}

public class BaseNodeGraphData : BaseNodeData
{
	List<NodeConnection> nodeConnections = new List<NodeConnection>();
	public List<NodeConnection> NodeConnections { get => nodeConnections; }

	public NodeConnection AddConnection(int connectToNodeId)
	{
		nodeConnections.Add( new NodeConnection( connectToNodeId ) );

		return nodeConnections[ nodeConnections.Count - 1 ];

	}

	public void RemoveNode( int connectionId )
	{
		nodeConnections.RemoveAt( connectionId );
	}

	public void RemoveNode( NodeConnection nodeConn )
	{
		nodeConnections.Remove( nodeConn );
	}
}

public class NodeConnection
{
	public int connectedNodeId;

	// TODO: When it comes to the bezier we should catch the start and end point so it is only updated when the position changes.
	Vector2 connectionStartPosition = Vector2.zero;
	Vector2 connectionEndPosition = Vector2.one;

	public NodeConnection(int connNodeId)
	{
		connectedNodeId = connNodeId;
	}

	public void DrawConnection(Vector2 startPosition, Vector2 endPosition)
	{
		if ( startPosition != connectionStartPosition || endPosition != connectionEndPosition )
		{
			connectionStartPosition = startPosition;
			connectionEndPosition = endPosition;
			Debug.LogWarning( "Nop, Nop, Nop..." );
		}

		Handles.DrawLine( connectionStartPosition, connectionEndPosition );
		Debug.Log( "DrawLine" );
	}

}