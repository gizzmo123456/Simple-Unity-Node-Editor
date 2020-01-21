using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public abstract class BaseNodeGraphEditor<T> : BaseNodeEditor<T> where T : BaseNodeGraphData
{

	public delegate void nodeConnection ( ConnectNodesStatus connectedStatus, int fromNodeId, int fromSlotId, int toNodeId, int toSlotId);
	public event nodeConnection NodeConnection;

	public enum ConnectNodesType { To, From, Cancel }
	public enum ConnectNodesStatus { Started, Canceled, Connected, Disconnected }

	protected Rect scrolViewInnerRect = Rect.zero;
	protected virtual int EdgeExtendMargin { get => 25; }
	protected virtual float EdgeExtendSpeed { get => 0.333f; }
	protected virtual float EdgeScrolSpeed { get => 2.5f; }
	protected int selectedNodeId = -1;

	protected override string NodeStyleName => "";

	bool invokedConnectingFromCallback = false;
	bool cancelConnection = false;
	int connectingFromNode = -1;    // < 0 is none 
	int connectingFromSlot = -1;
	int connectingToNode = -1;
	int connectingToSlot = -1;

	Vector2[] connectionPointsToMouse = new Vector2[ 21 ];

	public BaseNodeGraphEditor (int uid) : base(uid) { }
	public BaseNodeGraphEditor (int uid, Rect pannelRect) : base(uid, pannelRect) 
	{
		nodePressed += NodePressed;	
	}

	public override void Draw ( EditorWindow window )
	{
		base.Draw( window );

		DrawNodeConnections();
		ConnectNodes();

	}

	protected override void DrawNode ( int nodeId )
	{
		
		base.DrawNode( nodeId );

		if ( selectedNodeId == nodeId )
			ExtendScrolView( nodeId );

	}

	protected override Rect GetPannelViewRect ()
	{

		if ( scrolViewInnerRect == Rect.zero )
			scrolViewInnerRect = new Rect( Vector2.zero, panelRect.size - new Vector2(20, 20));

		return scrolViewInnerRect;
	}

	/// <summary>
	/// check if any node should extend the scrol view
	/// </summary>
	/// <param name="nodeId"></param>
	protected virtual void ExtendScrolView ( int nodeId )
	{

		Vector2 extendAmount = Vector2.zero;
		Vector2 nodePosition = GetNodePositionReleventToViewPort(nodeId);
		Vector2 nodeSize = nodes[ nodeId ].GetNodeSize();
		Vector2 moveNodeAmount = Vector2.zero;
		Vector2 scrolAmount = Vector2.zero;

		// extend the scrole view
		if ( nodePosition.x + nodeSize.x > scrolViewInnerRect.width - EdgeExtendMargin )
		{
			extendAmount.x = nodePosition.x + nodeSize.x - (scrolViewInnerRect.width - EdgeExtendMargin);

			moveNodeAmount.x -= extendAmount.x * EdgeExtendSpeed * Time.deltaTime;
			scrolAmount.x += extendAmount.x * EdgeScrolSpeed * Time.deltaTime;
		}
		else if( nodePosition.x < EdgeExtendMargin )
		{
			extendAmount.x = EdgeExtendMargin - nodePosition.x;

			moveNodeAmount.x += extendAmount.x * EdgeExtendSpeed * Time.deltaTime;
			moveNodeAmount.x += extendAmount.x * EdgeExtendSpeed;

			scrolAmount.x += extendAmount.x * EdgeScrolSpeed * Time.deltaTime;
		}

		if ( nodePosition.y + nodeSize.y > scrolViewInnerRect.height - EdgeExtendMargin )
		{
			extendAmount.y = nodePosition.y + nodeSize.y - ( scrolViewInnerRect.height - EdgeExtendMargin );

			moveNodeAmount.y += extendAmount.y * EdgeExtendSpeed * Time.deltaTime;
			scrolAmount.y += extendAmount.y * EdgeScrolSpeed * Time.deltaTime;
		}
		else if ( nodePosition.y < EdgeExtendMargin )
		{
			extendAmount.y = EdgeExtendMargin - extendAmount.y;

			moveNodeAmount.y += extendAmount.y * EdgeExtendSpeed * Time.deltaTime;
			moveNodeAmount.y += extendAmount.y * EdgeExtendSpeed;

			scrolAmount.y += extendAmount.y * EdgeScrolSpeed * Time.deltaTime;
		}

		extendAmount.x = Mathf.Max( 0, extendAmount.x );
		extendAmount.y = Mathf.Max( 0, extendAmount.y );

		scrolViewInnerRect.size += extendAmount * EdgeExtendSpeed;
		panelScrollPosition += scrolAmount;
		MoveAllNodes( moveNodeAmount - scrolAmount, nodeId );

	}

	public override T AddNode ( T data )
	{
		return base.AddNode( data );
	}

	public override void RemoveNode ( int id )
	{
		Debug.Log( "b4 Node Count: " + nodes.Count );
		base.RemoveNode( id ); 
		Debug.Log( "af Node Count: " + nodes.Count );

		// update all connection id
		for ( int i = 0; i < nodes.Count; i++ )
		{
			nodes[ i ].UpdateConnectingNodeIds( id );
		}
		
	}

	protected override Rect ClampNodePosition ( Rect nodeRect, int winId = 0 )
	{

		if ( nodeRect.x < panelRect.x - panelScrollPosition.x ) nodeRect.x = panelRect.x - panelScrollPosition.x;
		if ( nodeRect.y < panelRect.y - panelScrollPosition.y ) nodeRect.y = panelRect.y - panelScrollPosition.y;

		return nodeRect;
	}

	protected virtual void MoveAllNodes(Vector2 moveDelta, int except = -1 )
	{
		for ( int i = 0; i < nodes.Count; i++ )
		{
			if ( i == except ) continue;

			nodes[ i ].MoveNode(moveDelta);

		}
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

		if ( cancelConnection )
		{

			NodeConnection?.Invoke( ConnectNodesStatus.Canceled, connectingFromNode, connectingFromSlot, connectingToNode, connectingToSlot );
			ClearConnectNodes();

		}
		else if ( connectingToNode != -1 )
		{

			if ( nodes[ connectingFromNode ].HasConnection( connectingFromSlot, connectingToNode, connectingToSlot ) )	// Disconnect connection
			{
				nodes[ connectingFromNode ].RemoveConnection( connectingFromSlot, connectingToNode, connectingToSlot );
				NodeConnection?.Invoke( ConnectNodesStatus.Disconnected, connectingFromNode, connectingFromSlot, -1, -1 );
			}
			else // Compleat Connection.
			{
				nodes[ connectingFromNode ].AddConnection( connectingFromSlot, connectingToNode, connectingToSlot );
				NodeConnection?.Invoke( ConnectNodesStatus.Connected, connectingFromNode, connectingFromSlot, connectingToNode, connectingToSlot );
			}

			// reset connecting :)
			ClearConnectNodes();

		}
		else if ( connectingFromNode != -1 )	// start connection
		{
			// Draw curve to mouse.
			Color curveColour = nodes[ connectingFromNode ].NodeConnections_output[ connectingFromSlot ].pinColor;
			// Fake the output to mouse when connecting nodes.
			NodePin_Output curve = new NodePin_Output(0, null, "", nodes[ connectingFromNode ].BezierControlePointOffset, curveColour );

			Vector2 startPos = nodes[ connectingFromNode ].GetPinPosition( connectingFromSlot, BaseNodeGraphData.PinMode.Output ) + nodes[connectingFromNode].GetConnectionOffset( BaseNodeGraphData.PinMode.Output );
			Vector2 endPos = startPos;

			if ( Event.current != null )
				endPos = Event.current.mousePosition;

			curve.GenerateBezierCurve( startPos, endPos, ref connectionPointsToMouse );
			curve.DrawConnectionLines( connectionPointsToMouse, PositionIsVisable );

			// Signal that we have started a new connection.
			if ( !invokedConnectingFromCallback )
			{
				NodeConnection?.Invoke( ConnectNodesStatus.Started, connectingFromNode, connectingFromSlot, -1, -1 );
				invokedConnectingFromCallback = true;
			}

			repaint = true;
			
		}

	}

	/// <summary>
	/// Compleats the connection between connectingFromNode id
	/// </summary>
	/// <param name="connToNodeId"></param>
	/// <param name="connToSoltId"></param>
	/// <returns> False if not connecting from a node or node is out of range</returns>
	public bool CompleatNodeConnection( int connToNodeId, int connToSlotId )
	{
		if ( connectingFromNode < 0 )
		{
			Debug.LogError("Can not compleat connection, not connecting from a node.");
			return false;
		}
		if ( connToNodeId < 0 || connToNodeId >= nodes.Count )
		{
			Debug.LogErrorFormat("Can not compleat connection, 'to' node does not exist (MAX: {0} Current: {1})", ( nodes.Count - 1 ), connToNodeId );
			return false;
		}

		SetConnectNodes( ConnectNodesType.To, connToNodeId, connToSlotId );

		return true;
	}

	public void CancelNodeConnection()
	{
		if ( connectingFromNode < 0 && connectingToNode < 0 ) return; 

		cancelConnection = true;
	}

	private void SetConnectNodes(ConnectNodesType connectType, int nodeId, int slotId)
	{
		switch( connectType )
		{
			case ConnectNodesType.Cancel:
				cancelConnection = true;
			break;
			case ConnectNodesType.From:
				connectingFromNode = nodeId;
				connectingFromSlot = slotId;
			break;
			case ConnectNodesType.To:
				connectingToNode = nodeId;
				connectingToSlot = slotId;
			break;
		}
	}

	/// <summary>
	/// Clears the nodes being connecting. 
	/// DO NOT CALL to cancel the connection, since this will avoid triggering the callback.
	/// set 'CancelNodeConnection()' to cancel the current connection.
	/// </summary>
	protected void ClearConnectNodes()
	{
		connectingFromNode = connectingFromSlot = -1;
		connectingToNode = connectingToSlot = -1;
		invokedConnectingFromCallback = false;
		cancelConnection = false;
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

	protected virtual void NodePressed( int nodeId, Vector2 mousePosition, bool pressed )
	{

		if ( pressed )
		{
			selectedNodeId = nodeId;
			return;
		}
		else
		{
			selectedNodeId = -1;
		}

		int inputPinCount = nodes[ nodeId ].GetPinCount( BaseNodeGraphData.PinMode.Input );
		int outputPinCount = nodes[ nodeId ].GetPinCount( BaseNodeGraphData.PinMode.Output );

		// did we press an output pin of nodeId?
		for ( int i = 0; i < Mathf.Max(inputPinCount, outputPinCount); i++ )
			if (i < outputPinCount && nodes[nodeId].GetPinRect(i, BaseNodeGraphData.PinMode.Output).Contains(mousePosition))
			{
				if ( connectingFromNode < 0 )
				{
					SetConnectNodes( ConnectNodesType.From, nodeId, i );
					Debug.LogWarning( "Start Node Connection" );

				}
				else if ( connectingFromNode == nodeId && i == connectingFromSlot)
				{
					cancelConnection = true;
					Debug.LogWarning( "Connection Canceled" );

				}

			}
			else if (i < inputPinCount && connectingFromNode > -1 && connectingFromNode != nodeId && nodes[ nodeId ].GetPinRect( i, BaseNodeGraphData.PinMode.Input ).Contains( mousePosition ) )
			{
				SetConnectNodes( ConnectNodesType.To, nodeId, i );
				Debug.LogWarning( "Nodes Connected" );
			}
	}

}

public abstract class BaseNodeGraphData : BaseNodeData
{

	public enum PinMode { Input, Output }
	List<NodePin_Input> nodeConnections_input = new List<NodePin_Input>();
	List<NodePin_Output> nodeConnections_output = new List<NodePin_Output>();
	public List<NodePin_Input> NodeConnections_input { get => nodeConnections_input; }
	public List<NodePin_Output> NodeConnections_output { get => nodeConnections_output; }

	public int max_inputPins = -1;		// less than 0 == unlimited
	public int max_outputPins = -1;     // less than 0 == unlimited

	public Vector2 inputPin_localStartPosition = new Vector2( 0, 15f );
	public Vector2 outputPin_localStartPosition = new Vector2( 20, 15f );
	public Vector2 pinSize = new Vector2( 20, 18 );

	public virtual NodePin_Output.BezierControlePointOffset BezierControlePointOffset { get => NodePin_Output.BezierControlePointOffset.Horizontal; }

	public BaseNodeGraphData ( string _title, bool _dragable ) : base(_title, _dragable) {}
	public BaseNodeGraphData ( string _title, bool _dragable, int _max_inputPins, int _max_outputPins) : this( _title, _dragable )
	{
		max_inputPins = _max_inputPins;
		max_outputPins = _max_outputPins;

	}

	/// <param name="_inputStartPosition"> Local start position of input pins </param>
	/// <param name="_outputStartPosition"> Local start position of output pind </param>
	/// <param name="pinSize">size of pin</param>
	public BaseNodeGraphData ( string _title, bool _dragable, Vector2 _inputStartPosition, Vector2 _outputStartPosition, Vector2 _pinSize ) : base(_title, _dragable) 
	{
		inputPin_localStartPosition = _inputStartPosition;
		outputPin_localStartPosition = _outputStartPosition;
		pinSize = _pinSize;
	}

	protected override void GenerateNodeSize ()
	{
		Vector2 maxSize = new Vector2( 400, 800 );
		Vector2 nodeSize = new Vector2( 300, 30 );

		// get the node size by the amount of pins it has.
		float maxPinPos = Mathf.Max( GetPinLocalPosition( GetPinCount( BaseNodeGraphData.PinMode.Input ) - 1, BaseNodeGraphData.PinMode.Input ).y,
									 GetPinLocalPosition( GetPinCount( BaseNodeGraphData.PinMode.Output ) - 1, BaseNodeGraphData.PinMode.Output ).y );

		nodeSize.y += maxPinPos;

		rect.size = nodeSize;

	}

	public virtual void GeneratePinSizeAndPosition( Vector2 nodeSize ) // TODO: remove the param nodeSize, and use function GetNodeSize (from base class :/)
	{
		
		pinSize.x = nodeSize.x / 4f;

		inputPin_localStartPosition.x = -pinSize.x;
		inputPin_localStartPosition.y = 5;

		outputPin_localStartPosition.x = -pinSize.x + nodeSize.x - pinSize.x;
		outputPin_localStartPosition.y = 5;

	}

	/// <summary>
	/// Sets the max amount of inputs/outputs (pinMode) alowed ( <0 is unlimited)
	/// </summary>
	public void SetMaxPinCount ( PinMode pinMode, int maxPins )
	{
		switch (pinMode)
		{
			case PinMode.Input: 
				max_inputPins = maxPins;
			break;
			case PinMode.Output:
				max_outputPins = maxPins;
			break;
		}

	}

	/// <summary>
	/// Sets the max amount of inputs/outputs (pinMode) alowed ( <0 is unlimited)
	/// </summary>
	public void SetMaxPinCount( int maxInputs, int maxOutputs )
	{
		max_inputPins = maxInputs;
		max_outputPins = maxOutputs;
	}

	/// <summary>
	/// Are we able to add a pin or have we reached the pin limits
	/// </summary>
	/// <param name="pinMode"> pinMode to add to </param>
	/// <param name="displayMessage"> Should a error message be printed to console? </param>
	/// <returns> True if we can add a pin to pinMode else False</returns>
	public bool CanAddPin(PinMode pinMode, bool displayMessage = true)
	{
		int max_pins = pinMode == PinMode.Input ? max_inputPins : max_outputPins;

		if ( max_pins < 0 ) return true;    // less than 0 pins is unlimited pins

		int connections = pinMode == PinMode.Input ? nodeConnections_input.Count : nodeConnections_output.Count;

		if ( connections < max_pins ) return true;  // Less than limit

		if ( displayMessage )
			Debug.LogWarning("Unable to add pin, Pin limit reached for " + pinMode );

		return false;

	}

	public void AddPin (string connectionLable, PinMode pinMode) 
	{

		if ( !CanAddPin( pinMode ) ) return; 

		if ( pinMode == PinMode.Input )
			nodeConnections_input.Add( new NodePin_Input( nodeConnections_input.Count, this, connectionLable ) );
		else
			nodeConnections_output.Add( new NodePin_Output( nodeConnections_output.Count, this, connectionLable, BezierControlePointOffset ) );

	}

	public void AddOutputPin ( string connectionLable, Color pinColor )
	{

		if ( CanAddPin( PinMode.Output ) )
			nodeConnections_output.Add( new NodePin_Output( nodeConnections_output.Count, this, connectionLable, BezierControlePointOffset, pinColor ) );

	}

	public void SetOutputPinColor( int pinId, Color color)
	{
		nodeConnections_output[pinId].pinColor = color;
	}

	public void RemovePin ( int id, PinMode pinMode )
	{

		int pinCount = pinMode == PinMode.Input ? nodeConnections_input.Count : nodeConnections_output.Count;

		for ( int i = pinCount-1; i > id; i--)
			(pinMode == PinMode.Input ? nodeConnections_input[ i ] : nodeConnections_output[ i ]).id--;

		if ( pinMode == PinMode.Input )
			nodeConnections_input.RemoveAt( id );
		else
			nodeConnections_output.RemoveAt( id );

		
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

	/// <summary>
	/// Updated the output connection id, removing the affter id (intended for remove node)
	/// </summary>
	/// <param name="affterNodeId"> all nodes id's after this id will be updated </param>
	/// <param name="updateAmount"> the amount to update the node id by </param>
	public void UpdateConnectingNodeIds( int affterNodeId, int updateAmount = -1)	// would if be wort doing it in a range....
	{
		for (int outId = 0; outId < nodeConnections_output.Count; ++outId )
			nodeConnections_output[ outId ].UpdateConnectedNodeIds( affterNodeId, updateAmount, true );

	}

	/// <summary>
	/// The offset position from the pin that the connection should be drawn
	/// </summary>
	/// <returns></returns>
	public virtual Vector2 GetConnectionOffset(PinMode pinMode)
	{

		switch( pinMode )
		{
			case PinMode.Input:
				return new Vector2( 0, pinSize.y / 2f );
			case PinMode.Output:
				return new Vector2( pinSize.x, pinSize.y / 2f );
		}

		return Vector2.zero;

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

	public enum BezierControlePointOffset { Horizontal, Vertical }	//TODO: Bezier curve should have there own class :)

	List<NodeConnectionData> connections = new List<NodeConnectionData>();

	public bool alwaysForwardControlPoints = true;

	// Catch the start and end positions so we only update the curve when they change
	Vector2 connectionStartPosition = Vector2.zero;

	const int curvePoints = 20;
	const float curveWidth = 5;
	public BezierControlePointOffset bezierControleOffset = BezierControlePointOffset.Horizontal;
	public Color pinColor = Color.black;

	public NodePin_Output ( int _id, BaseNodeGraphData _ownerNode, string _connLable, BezierControlePointOffset _bezierControlePointOffset ) : base( _id, _ownerNode, _connLable ) 
	{ 
		bezierControleOffset = _bezierControlePointOffset;
	}

	public NodePin_Output ( int _id, BaseNodeGraphData _ownerNode, string _connLable, BezierControlePointOffset _bezierControlePointOffset, Color _pinColor ) : base( _id, _ownerNode, _connLable ) 
	{
		pinColor = _pinColor;
		bezierControleOffset = _bezierControlePointOffset;
	}

	public void SetBezierControlePoint( BezierControlePointOffset controlePointOffset )
	{

	}

	public void AddConnection(int nodeId, int slotId)
	{
		connections.Add( new NodeConnectionData( nodeId, slotId, curvePoints ) );
	}

	/// <summary>
	/// Updated the output connection id (intended for remove node)
	/// </summary>
	/// <param name="affterNodeId"> all nodes id's after this id will be updated </param>
	/// <param name="updateAmount"> the amount to update the node id by </param>
	/// <param name="removeAffterNodeId">Should the node at 'afterNodeId' be removed?</param>
	public void UpdateConnectedNodeIds( int affterNodeId, int updateAmount, bool removeAffterNodeId )
	{
		for ( int conId = connections.Count - 1; conId >= 0; --conId )
		{
			if ( removeAffterNodeId && affterNodeId == connections[conId].connectedNodeId)
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

		Vector2 curveStartPosition = ownerNode.GetPinPosition( id, BaseNodeGraphData.PinMode.Output ) + ownerNode.GetConnectionOffset( BaseNodeGraphData.PinMode.Output );

		if ( curveStartPosition != connectionStartPosition )
		{
			connectionStartPosition = curveStartPosition;
			connMoved = true;
		}
		
		for (int i = 0; i < connections.Count; i++)
		{
			NodeConnectionData connection = connections[ i ];
			// GenerateCurve if a node has moved.
			BaseNodeGraphData connectedNode = getNode( connection.connectedNodeId );

			if (connectedNode == null)
			{
				Debug.LogErrorFormat( "Unable to draw connection from node {0} to node {1}. Node {1} does not exist", id, connection.connectedNodeId );
				continue;
			}

			// skip if both nodes are not visable
			if ( !isVisable( ownerNode.NodeRect.position ) && !isVisable( connectedNode.NodeRect.position ) )
				continue;

			bool inConnMoved = connection.PinMoved( connectedNode.NodeRect.position );

			if ( connMoved || inConnMoved )
			{

				GenerateBezierCurve( curveStartPosition,
									 connectedNode.GetPinPosition( connection.connectedSlotId, BaseNodeGraphData.PinMode.Input ) + ownerNode.GetConnectionOffset( BaseNodeGraphData.PinMode.Input ), 
									 ref connection.connectionCurve );

				connection.SetStartPosition( connectedNode.NodeRect.position );  // Update start position.
				connections[ i ] = connection;

			}

			DrawConnectionLines(connections[i].connectionCurve, isVisable);

		}

	}

	public void GenerateBezierCurve(Vector2 from, Vector2 to, ref Vector2[] connectionCurve)
	{
		Vector2 offset = Vector2.zero;
		
		if (bezierControleOffset == BezierControlePointOffset.Horizontal)
			offset.x = ( to.x - from.x ) * (0.75f + (0.1f * (Mathf.Abs( to.y - from.y ) / 150f) ));
		else
			offset.y = ( to.y - from.y ) * ( 0.75f + ( 0.1f * ( Mathf.Abs( to.x - from.x ) / 150f ) ) );

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

	public void DrawConnectionLines(Vector2[] connectionPoints, isVisableFunct isVisable )
	{
		if ( connectionPoints.Length < 1 )
		{
			Debug.LogError("Error: Unable to draw conections with less than 2 point");
			return;
		}

		Color lastHandleColor = Handles.color;
		Color lineColour = pinColor;
		lineColour.a = 0.25f;
		Handles.color = pinColor;
		

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
				startPoint = new Vector3(connectionPoints[ i - 1 ].x, connectionPoints[ i - 1 ].y, 0) + Vector3.Cross( connectionPoints[ i - 1 ], startPoint );
				endPoint = new Vector3(connectionPoints[ i ].x, connectionPoints[ i ].y, 0) + Vector3.Cross( connectionPoints[ i ], endPoint );
				startPoint.z = 0;
				endPoint.z = 0;

				Vector3[] verts = { connectionPoints[ i - 1 ], connectionPoints[ i ], endPoint, startPoint };
				Handles.DrawSolidRectangleWithOutline( verts, lineColour, lineColour );
			}
		}

		Handles.color = lastHandleColor;	// put the color back

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

	public void SetNodeId( int newNodeId )
	{
		connectedNodeId = newNodeId;
	}

	public void UpdateNodeId( int amountToUpdate )
	{
		connectedNodeId += amountToUpdate;
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