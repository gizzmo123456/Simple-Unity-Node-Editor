﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public abstract class BaseNodeGraphEditor<T> : BaseNodeEditor<T> where T : BaseNodeGraphData
{

	public delegate void nodeConnection (bool connected, int fromNodeId, int fromSlotId, int toNodeId, int toSlotId);
	public event nodeConnection NodeConnection;

	protected override string NodeStyleName => "";


	bool repaint = false;

	int connectingFromNode = -1;    // < 0 is none 
	int connectingFromSlot = -1;
	int connectingToNode = -1;
	int connectingToSlot = -1;

	Vector2[] connectionPointsToMouse = new Vector2[ 21 ];

	public BaseNodeGraphEditor (int uid) : base(uid) { }
	public BaseNodeGraphEditor (int uid, Rect pannelRect) : base(uid, pannelRect) 
	{
		nodeReleased += NodeReleased;	
	}

	public override void Draw ( EditorWindow window )
	{
		base.Draw( window );

		DrawNodeConnections();
		ConnectNodes();

		if ( repaint )
		{
			window.Repaint();
			repaint = false;

		}
	}

	protected override Vector2 NodeSize ()
	{
		return new Vector2( 125, 100 );
	}

	protected override Vector2 NodeSize( int nodeId )
	{

		Vector2 maxSize = new Vector2(400, 800);
		Vector2 nodeSize = new Vector2( 300, 30 );

		// get the node size by the amount of pins it has.
		float maxPinPos = Mathf.Max( nodes[ nodeId ].GetPinLocalPosition( nodes[ nodeId ].GetPinCount( BaseNodeGraphData.PinMode.Input  ) - 1, BaseNodeGraphData.PinMode.Input  ).y,
									 nodes[ nodeId ].GetPinLocalPosition( nodes[ nodeId ].GetPinCount( BaseNodeGraphData.PinMode.Output ) - 1, BaseNodeGraphData.PinMode.Output ).y );



		nodeSize.y += maxPinPos;

		return nodeSize;

	}

	public override T AddNode ( T data )
	{
		return base.AddNode( data );
	}

	protected override Rect ClampNodePosition ( Rect nodeRect, int winId = 0 )
	{

		if ( nodeRect.x < panelRect.x - panelScrollPosition.x ) nodeRect.x = panelRect.x - panelScrollPosition.x;
		if ( nodeRect.y < panelRect.y - panelScrollPosition.y ) nodeRect.y = panelRect.y - panelScrollPosition.y;

		return nodeRect;
	}

	protected override void NodeWindow ( int windowId )
	{
		base.NodeWindow( windowId );

		int node_id = windowId - uniqueID;

		// keep repainting if the cursor is hovering over a node
		// should the pins are boxed are updated 
		if ( Event.current != null && Event.current.type != EventType.Repaint && Event.current.type != EventType.Layout )
		{	// BUG: this is not working correctly :(
			repaint = true;
		}

		DrawNodePins( node_id );

	}

	protected override Rect GetNodeContentsPosition ( int nodeId )
	{
		Vector2 nodeSize = nodes[ nodeId ].NodeRect.size;

		return new Rect()
		{
			x = nodeSize.x / 4f,
			y = 18,
			width = nodeSize.x / 2f,
			height = nodeSize.y - 36
		};

	}

	protected virtual void DrawNodePins(int node_id)
	{

		// inputs pins are drawn on the lhs of the node.
		// output pins are drawn on the rhs of the node.

		NodePin_Input[] nodeInputPins = nodes[ node_id ].NodeConnections_input.ToArray(); 
		NodePin_Input[] nodeOutputPins = nodes[ node_id ].NodeConnections_output.ToArray(); // connections output inherit from input and we only need the data that inputs class have.

		int maxPins = Mathf.Max( nodeOutputPins.Length, nodeInputPins.Length);

		for (int i = 0; i < maxPins; i++ )
		{
			string lable = "";
			Rect pinRect;

			if ( i < nodeInputPins.Length)
			{
				lable = nodeInputPins[ i ].connectionLable;
				pinRect = nodes[ node_id ].GetPinRect( i, BaseNodeGraphData.PinMode.Input );

				GUI.Box( pinRect, "" , guiSkin.GetStyle( "nodePin_box" ) );

				GUI.Label( pinRect, "#" );
				pinRect.x += 12;
				GUI.Label( pinRect, lable );
			}

			if ( i < nodeOutputPins.Length )
			{
				lable = nodeOutputPins[ i ].connectionLable;
				pinRect = nodes[ node_id ].GetPinRect( i, BaseNodeGraphData.PinMode.Output );

				GUI.Box( pinRect, "", guiSkin.GetStyle( "nodePin_box" ) );

				// add the width no the node pin (hash)  is on the right side of pinRect
				pinRect.x -= 3;
				GUI.Label( pinRect, "#", guiSkin.GetStyle( "nodeOutPin_text" ) );

				pinRect.x -= 12;
				GUI.Label( pinRect, lable, guiSkin.GetStyle( "nodeOutPin_text" ) );
			}

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
			
			foreach ( NodePin_Output nodeConn in nodes[i].NodeConnections_output )
			{
				nodeConn.DrawConnection(GetNode, PositionIsVisable);
			}
		}

	}

	protected virtual void ConnectNodes()
	{

		if ( connectingToNode != -1 )
		{

			if ( nodes[ connectingFromNode ].HasConnection( connectingFromSlot, connectingToNode, connectingToSlot ) )
			{
				nodes[ connectingFromNode ].RemoveConnection( connectingFromSlot, connectingToNode, connectingToSlot );
				NodeConnection?.Invoke( false, connectingFromNode, connectingFromSlot, connectingToNode, connectingToSlot );
			}
			else // Create a new connection.
			{
				nodes[ connectingFromNode ].AddConnection( connectingFromSlot, connectingToNode, connectingToSlot );
				NodeConnection?.Invoke( true, connectingFromNode, connectingFromSlot, connectingToNode, connectingToSlot );
			}

			Debug.LogWarning( string.Format( "Connecting from node {0}:{1} to node {2}:{3}", connectingFromNode, connectingFromSlot, connectingToNode, connectingToSlot ) );

			// reset connecting :)
			connectingFromNode = -1;   
			connectingFromSlot = -1;
			connectingToNode = -1;
			connectingToSlot = -1;

		}
		else if ( connectingFromNode != -1 )
		{
			// Draw curve to mouse.
			Color curveColour = nodes[ connectingFromNode ].NodeConnections_output[ connectingFromSlot ].pinColor;
			// Fake the output to mouse when connecting nodes.
			NodePin_Output curve = new NodePin_Output(0, null, "", curveColour);

			Vector2 startPos = nodes[ connectingFromNode ].GetPinPosition( connectingFromSlot, BaseNodeGraphData.PinMode.Output ) + new Vector2( nodes[ connectingFromNode ].pinSize.x, nodes[ connectingFromNode ].pinSize.y / 2f );
			Vector2 endPos = startPos;

			if ( Event.current != null )
				endPos = Event.current.mousePosition;

			curve.GenerateBezierCurve( startPos, endPos, ref connectionPointsToMouse );
			curve.DrawConnectionLines( connectionPointsToMouse, PositionIsVisable );

			repaint = true;
			
		}

	}

	/// <summary>
	/// Adds a new pin to node, resizeing the node if necessary
	/// </summary>
	public virtual void AddPin_toNode(int nodeId, string connectionLable, BaseNodeGraphData.PinMode pinMode)
	{
		nodes[ nodeId ].AddPin( connectionLable, pinMode );
		nodes[ nodeId ].SetNodeSize( NodeSize( nodeId ) );
		nodes[ nodeId ].GeneratePinSizeAndPosition( nodes[ nodeId ].NodeRect.size );
	}

	/// <summary>
	/// Adds a new output pin to node, resizeing the node if necessary
	/// </summary>
	public virtual void AddOutputPin_toNode ( int nodeId, string connectionLable, Color pinColor )
	{
		nodes[ nodeId ].AddOutputPin( connectionLable, pinColor );
		nodes[ nodeId ].SetNodeSize( NodeSize( nodeId ) );
		nodes[ nodeId ].GeneratePinSizeAndPosition( nodes[ nodeId ].NodeRect.size );
	}

	/// <summary>
	/// Removes a pin from node, resizeing the node if necessary
	/// </summary>
	/// <param name="nodeId"></param>
	/// <param name="connectionId"></param>
	/// <param name="pinMode"></param>
	public virtual void RemovePin_fromNode(int nodeId, int pinId, BaseNodeGraphData.PinMode pinMode)
	{
		nodes[ nodeId ].RemovePin( pinId, pinMode );
		nodes[ nodeId ].SetNodeSize( NodeSize( nodeId ) );
		nodes[ nodeId ].GeneratePinSizeAndPosition( nodes[ nodeId ].NodeRect.size );

	}

	protected virtual void NodeReleased( int nodeId, Vector2 mousePosition )
	{

		int inputPinCount = nodes[ nodeId ].GetPinCount( BaseNodeGraphData.PinMode.Input );
		int outputPinCount = nodes[ nodeId ].GetPinCount( BaseNodeGraphData.PinMode.Output );

		// did we press an output pin of nodeId?
		for ( int i = 0; i < Mathf.Max(inputPinCount, outputPinCount); i++ )
			if (i < outputPinCount && nodes[nodeId].GetPinRect(i, BaseNodeGraphData.PinMode.Output).Contains(mousePosition))
			{
				if ( connectingFromNode < 0 )
				{
					connectingFromNode = nodeId;
					connectingFromSlot = i;
					Debug.LogWarning( "yes" );

				}
				else if ( connectingFromNode == nodeId && i == connectingFromSlot)
				{
					connectingFromNode = -1;
					connectingFromSlot = -1;
					Debug.LogWarning( "No" );

				}

				Debug.LogWarning("Node: "+nodeId +" slot "+ i +" was pressed");

			}
			else if (i < inputPinCount && connectingFromNode > -1 && connectingFromNode != nodeId && nodes[ nodeId ].GetPinRect( i, BaseNodeGraphData.PinMode.Input ).Contains( mousePosition ) )
			{
				connectingToNode = nodeId;
				connectingToSlot = i;
			}
	}

}

public class BaseNodeGraphData : BaseNodeData
{

	public enum PinMode { Input, Output }
	List<NodePin_Input> nodeConnections_input = new List<NodePin_Input>();
	List<NodePin_Output> nodeConnections_output = new List<NodePin_Output>();
	public List<NodePin_Input> NodeConnections_input { get => nodeConnections_input; }
	public List<NodePin_Output> NodeConnections_output { get => nodeConnections_output; }

	public Vector2 inputPin_localStartPosition = new Vector2( 0, 15f );
	public Vector2 outputPin_localStartPosition = new Vector2( 20, 15f );
	public Vector2 pinSize = new Vector2( 20, 18 );

	public BaseNodeGraphData ( string _title, bool _dragable ) : base(_title, _dragable) {}

	/// <param name="_inputStartPosition"> Local start position of input pins </param>
	/// <param name="_outputStartPosition"> Local start position of output pind </param>
	/// <param name="pinSize">size of pin</param>
	public BaseNodeGraphData ( string _title, bool _dragable, Vector2 _inputStartPosition, Vector2 _outputStartPosition, Vector2 _pinSize ) : base(_title, _dragable) 
	{
		inputPin_localStartPosition = _inputStartPosition;
		outputPin_localStartPosition = _outputStartPosition;
		pinSize = _pinSize;
	}

	public void GeneratePinSizeAndPosition( Vector2 nodeSize )
	{

		pinSize.x = nodeSize.x / 4f;

		inputPin_localStartPosition.x = -pinSize.x;
		inputPin_localStartPosition.y = 5;

		outputPin_localStartPosition.x = -pinSize.x + nodeSize.x - pinSize.x;
		outputPin_localStartPosition.y = 5;

	}

	public void AddPin (string connectionLable, PinMode pinMode) 
	{
		if ( pinMode == PinMode.Input )
			nodeConnections_input.Add( new NodePin_Input( nodeConnections_input.Count, this, connectionLable ) );
		else
			nodeConnections_output.Add( new NodePin_Output( nodeConnections_output.Count, this, connectionLable ) );

	}

	public void AddOutputPin ( string connectionLable, Color pinColor )
	{

		nodeConnections_output.Add( new NodePin_Output( nodeConnections_output.Count, this, connectionLable, pinColor ) );

	}

	public void SetOutputPinColor( int pinId, Color color)
	{
		nodeConnections_output[pinId].pinColor = color;
	}

	public void RemovePin<T> (T node) where T : NodePin_Input 
	{
		if ( node is NodePin_Input )
			nodeConnections_input.Remove( node );
		else if ( node is NodePin_Output )
			nodeConnections_output.Remove( node as NodePin_Output );
		else
			Debug.LogError( "Error, can not remove pin NodeConnection type not found!" );
	}

	public void RemovePin ( int id, PinMode pinMode )
	{
		if ( pinMode == PinMode.Input )
			nodeConnections_input.RemoveAt(id);
		else
			nodeConnections_output.RemoveAt(id);

	}

	public int GetPinCount( PinMode pinMode )
	{
		if ( pinMode == PinMode.Input )
			return nodeConnections_input.Count;
		else if ( pinMode == PinMode.Output )
			return nodeConnections_output.Count;
		else
			return 0;
	}

	protected virtual Vector2 GetPinOffset( int pinId, PinMode pinMode )
	{
		Vector2 pinOffset = pinSize;
		pinOffset.y += pinOffset.y * pinId;
		
		return pinOffset;
		
	}
	
	public Vector2 GetPinLocalPosition ( int pinId, PinMode pinMode )
	{
		Vector2 pinOffset = GetPinOffset(pinId, pinMode);

		if ( pinMode == PinMode.Input )
			return inputPin_localStartPosition + pinOffset;
		else if ( pinMode == PinMode.Output )
			return outputPin_localStartPosition + pinOffset;
		else
			Debug.LogError( "Error: Pin mode not found!" );
		return rect.position;

	}

	/// <summary>
	/// Gets the position of the pin in editorWindow space :)
	/// </summary>
	/// <param name="pinId"></param>
	/// <param name="pinMode"></param>
	/// <returns></returns>
	public Vector2 GetPinPosition(int pinId, PinMode pinMode)
	{
		Vector2 pinOffset = GetPinOffset( pinId, pinMode );

		if ( pinMode == PinMode.Input )
			return rect.position + inputPin_localStartPosition + pinOffset;
		else if ( pinMode == PinMode.Output )
			return rect.position + outputPin_localStartPosition + pinOffset;
		else
			Debug.LogError( "Error: Pin mode not found!" );
			return rect.position;

	}

	public Rect GetPinRect (int pinId, PinMode pinMode)
	{
		return new Rect( GetPinLocalPosition( pinId, pinMode ), pinSize );
	}

	public void AddConnection (int from_pinId, int toNodeId, int toSlotId)
	{
		nodeConnections_output[ from_pinId ].AddConnection( toNodeId, toSlotId );
	}

	public bool HasConnection( int from_pinId, int toNodeId, int toSlotId )
	{

		return nodeConnections_output[ from_pinId ].HasConnection( toNodeId, toSlotId );		

	}

	public void RemoveConnection( int pinId, int toNodeId, int toNodeSlot )
	{
		nodeConnections_output[ pinId ].RemoveConnection( toNodeId, toNodeSlot );
	}

	public int GetConnectionCount( PinMode pinMode, int slotId )
	{
		return 0;
	}

	public bool CanConnect()
	{
		return false;
	}

}

public class NodePin_Input
{
	public int id;
	public BaseNodeGraphData ownerNode;
	public string connectionLable;

	public NodePin_Input(int _id, BaseNodeGraphData _ownerNode, string connLable)
	{
		id = _id;
		ownerNode = _ownerNode;
		connectionLable = connLable;
	}

}

public class NodePin_Output : NodePin_Input
{
	public delegate bool isVisableFunct ( Vector2 position );
	public delegate BaseNodeGraphData getNodeFunct ( int nodeId );

	List<NodeConnectionData> connections = new List<NodeConnectionData>();

	public bool alwaysForwardControlPoints = true;

	// Catch the start and end positions so we only update the curve when they change
	Vector2 connectionStartPosition = Vector2.zero;

	const int curvePoints = 20;
	const float curveWidth = 5;
	public Color pinColor = Color.black;

	public NodePin_Output ( int _id, BaseNodeGraphData _ownerNode, string _connLable ) : base( _id, _ownerNode, _connLable ) { }
	public NodePin_Output ( int _id, BaseNodeGraphData _ownerNode, string _connLable, Color _pinColor ) : base( _id, _ownerNode, _connLable ) 
	{
		pinColor = _pinColor;
	}

	public void AddConnection(int nodeId, int slotId)
	{
		connections.Add( new NodeConnectionData( nodeId, slotId, curvePoints ) );
	}

	public bool HasConnection(int nodeId, int slotId)
	{
		foreach ( NodeConnectionData conn in connections )
			if ( conn.connectedNodeId == nodeId && conn.connectedSlotId == slotId )
				return true;

		return false;

	}

	public void RemoveConnection(int inputNodeId, int slotId)
	{
		for ( int i = 0; i < connections.Count; i++ )
			if ( connections[i].connectedNodeId == inputNodeId && connections[i].connectedSlotId == slotId )
			{
				connections.RemoveAt( i );
				return;
			}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="startPosition"></param>
	/// <param name="endPosition"></param>
	/// <param name="isVisable"></param>
	public void DrawConnection(getNodeFunct getNode, isVisableFunct isVisable )
	{

		bool connMoved = false;

		if ( ownerNode.GetPinPosition(0, BaseNodeGraphData.PinMode.Output) != connectionStartPosition )
		{
			connectionStartPosition = ownerNode.GetPinPosition( 0, BaseNodeGraphData.PinMode.Output );
			connMoved = true;
		}
		
		for (int i = 0; i < connections.Count; i++)
		{
			NodeConnectionData connection = connections[ i ];
			// GenerateCurve if a node has moved.
			BaseNodeGraphData connectedNode = getNode( connection.connectedNodeId );

			// skip if both nodes are not visable
			if ( !isVisable( ownerNode.NodeRect.position ) && !isVisable( connectedNode.NodeRect.position ) )
				continue;

			bool inConnMoved = connection.PinMoved( connectedNode.NodeRect.position );

			if ( connMoved || inConnMoved )
			{

				GenerateBezierCurve( ownerNode.GetPinPosition( id, BaseNodeGraphData.PinMode.Output ) + new Vector2( ownerNode.pinSize.x, ownerNode.pinSize.y / 2f ), 
									 connectedNode.GetPinPosition( connection.connectedSlotId, BaseNodeGraphData.PinMode.Input ) + new Vector2( 0, ownerNode.pinSize.y / 2f ), 
									 ref connection.connectionCurve );  //NOTE: Make sure this remembers the curve :/

				connection.SetStartPosition( connectedNode.NodeRect.position );  // Update start position.
				connections[ i ] = connection;

			}

			DrawConnectionLines(connections[i].connectionCurve, isVisable);

		}

	}

	public void GenerateBezierCurve(Vector2 from, Vector2 to, ref Vector2[] connectionCurve)
	{
		float xOffset = ( to.x - from.x ) * (0.75f + (0.1f * (Mathf.Abs( to.y - from.y ) / 150f) )) ;

		if ( alwaysForwardControlPoints )
			xOffset = Mathf.Abs( xOffset );

		Vector2 fromControlPoint = from + new Vector2( xOffset, 0 );
		Vector2 toControlPoint = to + new Vector2( -xOffset, 0 );

		for ( int i = 0; i < curvePoints + 1; i++ ) 
		{
			float t = (float)i / (float)curvePoints;
			connectionCurve[ i ] = ( Mathf.Pow( 1f - t, 3 ) * from ) +
								   ( 3f * Mathf.Pow( 1f - t, 2 ) * t * fromControlPoint ) + 
								   ( 3f * ( 1 - t ) * Mathf.Pow( t, 2 ) * toControlPoint ) + 
								   ( Mathf.Pow( t, 3 ) * to );

		}
	}

	public void DrawConnectionLines(Vector2[] connectionPoints, isVisableFunct isVisable )
	{
		if ( connectionPoints.Length < 1 )
		{
			Debug.LogError("Error: Unable to draw conections with less than 2 point");
			return;
		}

		Color lineColour = pinColor;
		lineColour.a = 0.25f;
		Handles.color = pinColor ;
		

		for ( int i = 1; i < connectionPoints.Length; i++ )
		{
			Vector2 lineCenter = connectionPoints[ i - 1 ] + ( ( connectionPoints[ i ] - connectionPoints[ i - 1 ] ) / 2f );

			if ( isVisable( lineCenter ) )
			{
				// TODO: improve line width
				Vector3 startPoint = connectionPoints[ i - 1 ];
				Vector3 endPoint = connectionPoints[ i ];
				startPoint.z = curveWidth;
				endPoint.z = curveWidth;
				startPoint.Normalize();
				endPoint.Normalize();
				startPoint = new Vector3(connectionPoints[ i - 1 ].x, connectionPoints[ i - 1 ].y, 0) + (Vector3.Cross( connectionPoints[ i - 1 ], startPoint ));
				endPoint = new Vector3(connectionPoints[ i ].x, connectionPoints[ i ].y, 0) + Vector3.Cross( connectionPoints[ i ], endPoint );
				startPoint.z = 0;
				endPoint.z = 0;
				//Handles.DrawLine( connectionPoints[ i - 1 ], connectionPoints[ i ] );

				Vector3[] verts = { connectionPoints[ i - 1 ], connectionPoints[ i ], endPoint, startPoint };
				Handles.DrawSolidRectangleWithOutline( verts, lineColour, lineColour );
			}
		}

	}

}

struct NodeConnectionData
{
	public int connectedNodeId;     // the node id to connect to.
	public int connectedSlotId;     // the input slot that the node is connected to.

	public Vector2[] connectionCurve;

	public Vector2 inputPin_startPosition;

	public NodeConnectionData(int connNodeId, int connSlotId, int curvePoints)
	{
		connectedNodeId = connNodeId;
		connectedSlotId = connSlotId;
		connectionCurve = new Vector2[ curvePoints + 1 ];
		inputPin_startPosition = Vector2.zero;
		Debug.LogWarning( "???" );

	}

	public void SetStartPosition( Vector2 position )
	{
		inputPin_startPosition = position;
	}

	public bool PinMoved(Vector2 position)
	{
		bool moved = position != inputPin_startPosition;


		return moved;
	}

}