using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NodePin_Input
{
	public int id;
	public BaseNodeGraphData ownerNode;
	public string connectionLable;

	protected virtual NodeConnectionType PinConnectionType => NodeConnectionType.Input;
	protected List<NodeConnectionData> connections = new List<NodeConnectionData>();
	public NodeConnectionData this[int indx] => connections[indx];

	public int ConnectionCount => connections.Count;
	public int ConnectionLimit { get; private set; }    // if limit is < 0 there is no limit.

	public NodePin_Input ( int _id, BaseNodeGraphData _ownerNode, string _connLable, int _connLimit = -1 )
	{
		id = _id;
		ownerNode = _ownerNode;
		connectionLable = _connLable;
		ConnectionLimit = _connLimit;

	}

	public virtual bool AddConnection ( int nodeId, int slotId )
	{
		if ( CanConnect() )
		{
			connections.Add( new NodeConnectionData( PinConnectionType, nodeId, slotId ) );
			return true;
		}
		
		return false;
		
	}

	/// <summary>
	/// Updated the output connection id (intended for remove node)
	/// </summary>
	/// <param name="affterNodeId"> all nodes id's after this id will be updated </param>
	/// <param name="updateAmount"> the amount to update the node id by </param>
	/// <param name="removeAffterNodeId">Should the node at 'afterNodeId' be removed?</param>
	public void UpdateConnectedNodeIds ( int affterNodeId, int updateAmount, bool removeAffterNodeId )
	{
		for ( int conId = connections.Count - 1; conId >= 0; --conId )
		{
			if ( removeAffterNodeId && affterNodeId == connections[ conId ].connectedNodeId )
			{
				connections.RemoveAt( conId );
			}
			else if ( connections[ conId ].connectedNodeId > affterNodeId )
			{
				NodeConnectionData conData = connections[ conId ];
				conData.UpdateNodeId( updateAmount );
				connections[ conId ] = conData;

			}

		}

	}

	public bool CanConnect ()
	{
		return ConnectionLimit < 0 || ConnectionCount < ConnectionLimit;
	}

	/// <summary>
	/// Find if this pin has a connection to node and pin
	/// </summary>
	/// <returns> True if found Flase otherwise</returns>
	public bool HasConnection ( int nodeId, int slotId )
	{
		foreach ( NodeConnectionData conn in connections )
			if ( conn.connectedNodeId == nodeId && conn.connectedSlotId == slotId )
				return true;

		return false;

	}

	/// <summary>
	/// Remove the connection from the pin
	/// </summary>
	public void RemoveConnection ( int inputNodeId, int slotId )
	{
		for ( int i = 0; i < connections.Count; i++ )
			if ( connections[ i ].connectedNodeId == inputNodeId && connections[ i ].connectedSlotId == slotId )
			{
				connections.RemoveAt( i );
				return;
			}
	}

}

public class NodePin_Output : NodePin_Input
{
	public delegate bool isVisableFunct ( Vector2 position );
	public delegate BaseNodeGraphData getNodeFunct ( int nodeId );

	public enum BezierControlePointOffset { Horizontal, Vertical }  //TODO: Bezier curve should have there own class :)
	protected override NodeConnectionType PinConnectionType => NodeConnectionType.Output;

	public bool alwaysForwardControlPoints = true;

	// Catch the start and end positions so we only update the curve when they change
	Vector2 connectionStartPosition = Vector2.zero;

	const int curvePoints = 20;
	const float curveWidth = 5;
	public BezierControlePointOffset bezierControleOffset = BezierControlePointOffset.Horizontal;
	public Color pinColor = Color.black;

	public NodePin_Output ( int _id, BaseNodeGraphData _ownerNode, string _connLable, BezierControlePointOffset _bezierControlePointOffset, int _connLimit = -1 ) : base( _id, _ownerNode, _connLable, _connLimit )
	{
		bezierControleOffset = _bezierControlePointOffset;
	}

	public NodePin_Output ( int _id, BaseNodeGraphData _ownerNode, string _connLable, BezierControlePointOffset _bezierControlePointOffset, Color _pinColor, int _connLimit = -1 ) : base( _id, _ownerNode, _connLable, _connLimit )
	{
		pinColor = _pinColor;
		bezierControleOffset = _bezierControlePointOffset;
	}

	public void SetBezierControlePoint ( BezierControlePointOffset controlePointOffset )
	{

	}

	/// <summary>
	/// Addes a new connection to the pin
	/// </summary>
	/// <param name="nodeId"> the node to connect to </param>
	/// <param name="slotId"> the slot to connection on the node </param>
	public override bool AddConnection ( int nodeId, int slotId )
	{
		if ( CanConnect() )
		{
			connections.Add( new NodeConnectionData( PinConnectionType, nodeId, slotId, curvePoints ) );
			return true;
		}

		return false;
	}

	private bool IsCurveVisableInDrawArea( Rect drawArea, Vector2 startPos, Vector2 endPos )
	{

		Rect curveBounds = new Rect( startPos.Min( endPos ), startPos.Max( endPos ) - startPos.Min( endPos ) );

		// is either the draw area with in the curve bounds or curve bounds within the draw area.
		return drawArea.Contains( curveBounds.position ) || drawArea.Contains( curveBounds.position + curveBounds.size ) ||
			   curveBounds.Contains( drawArea.position ) || curveBounds.Contains( drawArea.position + drawArea.size );

	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="startPosition"></param>
	/// <param name="endPosition"></param>
	/// <param name="isVisable"></param>
	public void DrawConnection ( Rect drawArea, getNodeFunct getNode ) //, isVisableFunct isVisable )
	{

		bool connMoved = false;

		Vector2 curveStartPosition = ownerNode.GetPinPosition( id, BaseNodeGraphData.PinMode.Output ) + ownerNode.GetConnectionOffset( BaseNodeGraphData.PinMode.Output );

		if ( curveStartPosition != connectionStartPosition )
		{
			connectionStartPosition = curveStartPosition;
			connMoved = true;
		}

		for ( int i = 0; i < connections.Count; i++ )
		{

			NodeConnectionData connection = connections[ i ];

			// GenerateCurve if a node has moved.
			BaseNodeGraphData connectedNode = getNode( connection.connectedNodeId );
			Vector2 curveEndPosition = connectedNode.GetPinPosition( connection.connectedSlotId, BaseNodeGraphData.PinMode.Input ) + connectedNode.GetConnectionOffset( BaseNodeGraphData.PinMode.Input );

			if ( connectedNode == null )
			{
				Debug.LogErrorFormat( "Unable to draw connection from node {0} to node {1}. Node {1} does not exist", id, connection.connectedNodeId );
				continue;
			}

			// skip if both nodes are not visable
			//if ( !isVisable( ownerNode.NodeRect.position ) && !isVisable( connectedNode.NodeRect.position ) )
			//	continue;

			if ( !IsCurveVisableInDrawArea( drawArea, curveStartPosition, curveEndPosition ) )
				continue;

			bool inConnMoved = connection.PinMoved( connectedNode.NodeRect.position );

			if ( connMoved || inConnMoved )
			{

				GenerateBezierCurve( curveStartPosition,
									 curveEndPosition  ,
									 ref connection.connectionCurve );

				connection.SetStartPosition( connectedNode.NodeRect.position );  // Update start position.
				connections[ i ] = connection;

			}

			DrawConnectionLines( connections[ i ].connectionCurve, drawArea );

		}

	}

	public void GenerateBezierCurve ( Vector2 from, Vector2 to, ref Vector2[] connectionCurve )
	{
		Vector2 offset = Vector2.zero;

		float xDif = to.x - from.x;
		float yDif = to.y - from.y;

		if ( bezierControleOffset == BezierControlePointOffset.Horizontal )
			offset.x = xDif * ( 0.35f + (0.1f * Mathf.Abs( yDif ) / 150f ) );
		else
			offset.y = yDif * ( 0.35f + (0.1f * Mathf.Abs( xDif ) / 150f ) );

		if ( alwaysForwardControlPoints )
			offset = offset.Abs();

		Vector2 fromControlPoint = from + offset;
		Vector2 toControlPoint = to - offset;

		for ( int i = 0; i < curvePoints + 1; i++ )
		{
			float t = (float)i / (float)curvePoints;
			connectionCurve[ i ] = ( Mathf.Pow( 1f - t, 3 ) * from ) +
								   ( 3f * Mathf.Pow( 1f - t, 2 ) * t * fromControlPoint ) +
								   ( 3f * ( 1 - t ) * Mathf.Pow( t, 2 ) * toControlPoint ) +
								   ( Mathf.Pow( t, 3 ) * to );

		}
	}

	public void DrawConnectionLines ( Vector2[] connectionPoints, Rect drawArea )
	{
		if ( connectionPoints.Length < 1 )
		{
			Debug.LogError( "Error: Unable to draw conections with less than 2 point" );
			return;
		}

		Color lastHandleColor = Handles.color;
		Color lineColour = pinColor;
		lineColour.a = 0.25f;
		Handles.color = pinColor;

		for ( int i = 1; i < connectionPoints.Length; i++ )
		{

			Vector2 startPoint = connectionPoints[ i - 1 ];
			Vector2 endPoint = connectionPoints[ i ];

			if ( drawArea.Contains( startPoint ) || drawArea.Contains( endPoint ) )
			{

				Vector2 vect = endPoint - startPoint;
				Vector2 perb = Vector2.Perpendicular( vect.normalized * curveWidth );

				Vector3[] verts = { connectionPoints[ i - 1 ], connectionPoints[ i ], endPoint + perb, startPoint + perb };
				Handles.DrawSolidRectangleWithOutline( verts, lineColour, lineColour );

			}


		}

		Handles.color = lastHandleColor;    // put the color back

	}

}
