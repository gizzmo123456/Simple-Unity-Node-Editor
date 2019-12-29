using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BaseNodeGraphEditor : BaseNodeEditor<BaseNodeGraphData>
{
	public BaseNodeGraphEditor () : base() { }
	public BaseNodeGraphEditor (Rect pannelRect) : base(pannelRect) { }

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


}

public class BaseNodeGraphData : BaseNodeData
{
	List<NodeConnection> nodeConnections = new List<NodeConnection>();

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
	
	public NodeConnection(int connNodeId)
	{
		connectedNodeId = connNodeId;
	}
}