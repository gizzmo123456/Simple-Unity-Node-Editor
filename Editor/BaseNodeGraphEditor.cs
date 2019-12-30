﻿using System.Collections;
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
	}

////////// Node Connections
	protected virtual void DrawNodeConnections()
	{

		for( int i = 0; i < nodes.Count; i++ )
		{
			
			foreach ( NodeConnection nodeConn in nodes[i].NodeConnections )
			{
				Vector2 startPos = GetConnectionPosition( i, true, nodeConn );
				Vector2 endPos = GetConnectionPosition( nodeConn.connectedNodeId, false, nodeConn );

				if ( !PositionIsVisable( startPos ) && !PositionIsVisable( endPos ) ) continue; // do not draw connect if both start and end points are not visable

				nodeConn.DrawConnection(startPos, endPos, PositionIsVisable);
			}
		}

	}

	protected virtual Vector2 GetConnectionPosition(int nodeId, bool output, NodeConnection connectionData)
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
	List<NodeConnection> nodeConnections = new List<NodeConnection>();
	public List<NodeConnection> NodeConnections { get => nodeConnections; }

	public NodeConnection AddConnection(int connectToNodeId)
	{
		nodeConnections.Add( new NodeConnection( nodeConnections.Count, connectToNodeId ) );

		return nodeConnections[ nodeConnections.Count - 1 ];

	}

	public void RemoveConnection( int connectionId )
	{
		nodeConnections.RemoveAt( connectionId );
	}

	public void RemoveConnection( NodeConnection nodeConn )
	{
		nodeConnections.Remove( nodeConn );
	}
}

public class NodeConnection
{
	public delegate bool isVisableFunct ( Vector2 position );
	
	public int id;
	public int connectedNodeId;

	public bool alwaysForwardControlPoints = true;

	public Color curveColor = Color.black;

	// Catch the start and end positions so we only update the curve when they change
	Vector2 connectionStartPosition = Vector2.zero;
	Vector2 connectionEndPosition = Vector2.one;

	const int curvePoints = 20;
	Vector2[] connectionCurve = new Vector2[ curvePoints + 1 ];

	public NodeConnection(int _id, int connNodeId)
	{
		id = _id;
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