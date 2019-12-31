using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BaseNodeGraphEditor : BaseNodeEditor<BaseNodeGraphData>
{
	public BaseNodeGraphEditor (int uid) : base(uid) { }
	public BaseNodeGraphEditor (int uid, Rect pannelRect) : base(uid, pannelRect) 
	{
		nodeReleased += NodeReleased;	
	}

	public override void Draw ( EditorWindow window )
	{
		base.Draw( window );

		DrawNodeConnections();

	}

	protected override Vector2 NodeSize ()
	{
		return new Vector2( 125, 100 );
	}

	protected override Vector2 NodeSize( int nodeId )
	{

		Vector2 maxSize = new Vector2(400, 800);
		Vector2 nodeSize = new Vector2( 300, 20 );

		// get the node size by the amount of connections is has.
		float maxPinPos = Mathf.Max( nodes[ nodeId ].GetPinLocalPosition( nodes[ nodeId ].GetConnectionCount( BaseNodeGraphData.PinMode.Input  ) - 1, BaseNodeGraphData.PinMode.Input  ).y,
									 nodes[ nodeId ].GetPinLocalPosition( nodes[ nodeId ].GetConnectionCount( BaseNodeGraphData.PinMode.Output ) - 1, BaseNodeGraphData.PinMode.Output ).y );



		nodeSize.y += maxPinPos;

		return nodeSize;

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

		DrawNodePins( node_id );

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
				pinRect.y -= 5;
				GUI.Label( pinRect, "#" );
				pinRect.x += 12;
				GUI.Label( pinRect, lable );
			}

			if ( i < nodeOutputPins.Length )
			{
				lable = nodeOutputPins[ i ].connectionLable;
				pinRect = nodes[ node_id ].GetPinRect( i, BaseNodeGraphData.PinMode.Output );
				pinRect.y -= 5;
				pinRect.x -= 15;
				GUI.Label( pinRect, "#" );
				pinRect.x -= pinRect.width + 10;
				GUI.Label( pinRect, lable );
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

	/// <summary>
	/// Adds a new pin to node, resizeing the node if necessary
	/// </summary>
	/// <param name="nodeId"></param>
	/// <param name="connectionLable"></param>
	/// <param name="pinMode"></param>
	public virtual void AddPin_toNode(int nodeId, string connectionLable, BaseNodeGraphData.PinMode pinMode)
	{
		nodes[ nodeId ].AddPin( connectionLable, pinMode );
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

		// did we press an output pin of nodeId?
		for ( int i = 0; i < nodes[nodeId].GetPinCount(BaseNodeGraphData.PinMode.Output); i++ )
			if (nodes[nodeId].GetPinRect(i, BaseNodeGraphData.PinMode.Output).Contains(mousePosition))
			{
				// 
			}
			
			
			//.Contains(mousePosition) )

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

	public void GeneratePinSizeAndPosition( Vector2 nodeSize )
	{

		pinSize.x = nodeSize.x / 4f;

		inputPin_localStartPosition.x = -pinSize.x + 3f;
		inputPin_localStartPosition.y = 5;

		outputPin_localStartPosition.x =  nodeSize.x - pinSize.x;
		outputPin_localStartPosition.y = 5;

	}

	public void AddPin (string connectionLable, PinMode pinMode) 
	{
		if ( pinMode == PinMode.Input )
			nodeConnections_input.Add( new NodePin_Input( nodeConnections_input.Count, this, connectionLable ) );
		else
			nodeConnections_output.Add( new NodePin_Output( nodeConnections_output.Count, this, connectionLable ) );

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

	public Vector2 GetPinLocalPosition ( int pinId, PinMode pinMode )
	{
		Vector2 pinOffset = pinSize;
		pinOffset.y += pinOffset.y * pinId;

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
		Vector2 pinOffset = pinSize;
		pinOffset.y += pinOffset.y * pinId;

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

	// TODO: ATM this will remove the pin it needs to be updated to remove the connection :)
	public void RemoveConnection( int pinId, int connectionId )
	{
		nodeConnections_output[ pinId ].RemoveConnection( connectionId );
	}

	public int GetConnectionCount( PinMode pinMode )
	{
		return pinMode == PinMode.Input ? nodeConnections_input.Count : nodeConnections_output.Count;
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
	public Color curveColor = Color.black;

	public NodePin_Output ( int _id, BaseNodeGraphData _ownerNode, string connLable ) : base( _id, _ownerNode, connLable ) { }

	public void AddConnection(int nodeId, int slotId)
	{
		connections.Add( new NodeConnectionData( nodeId, slotId, curvePoints ) );
	}

	public void RemoveConnection(int connId)
	{
		if ( connId >= 0 && connId < connections.Count )
			connections.RemoveAt( connId );
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

		Handles.color = curveColor;
		
		for (int i = 0; i < connections.Count; i++)
		{
			NodeConnectionData connection = connections[ i ];
			// GenerateCurve if a node has moved.
			BaseNodeGraphData connectedNode = getNode( connection.connectedNodeId );

			// skip if both nodes are not visable
			if ( !isVisable( ownerNode.NodeRect.position ) && !isVisable( connectedNode.NodeRect.position ) )
				continue;

			if ( connMoved || connection.PinMoved( connectedNode.GetPinPosition( 0, BaseNodeGraphData.PinMode.Input ), true ) )
			{
				GenerateBezierCurve( ownerNode.GetPinPosition( id, BaseNodeGraphData.PinMode.Output ), 
									 connectedNode.GetPinPosition( connection.connectedSlotId, BaseNodeGraphData.PinMode.Input ), 
									 ref connection.connectionCurve );	//NOTE: Make sure this remembers the curve :/
			}

			DrawConnection(connections[i].connectionCurve, isVisable);

		}

	}

	private void GenerateBezierCurve(Vector2 from, Vector2 to, ref Vector2[] connectionCurve)
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

	private void DrawConnection(Vector2[] connectionPoints, isVisableFunct isVisable )
	{
		if ( connectionPoints.Length < 1 )
		{
			Debug.LogError("Error: Unable to draw conections with less than 2 point");
			return;
		}

		for ( int i = 1; i < connectionPoints.Length; i++ )
		{
			Vector2 lineCenter = connectionPoints[ i - 1 ] + ( ( connectionPoints[ i ] - connectionPoints[ i - 1 ] ) / 2f );

			if ( isVisable( lineCenter ) )
				Handles.DrawLine( connectionPoints[ i - 1 ], connectionPoints[ i ] );
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
	}

	public bool PinMoved(Vector2 position, bool updatePosition = false)
	{
		bool moved = position != inputPin_startPosition;

		if ( moved && updatePosition )
			inputPin_startPosition = position;

		return moved;
	}

}