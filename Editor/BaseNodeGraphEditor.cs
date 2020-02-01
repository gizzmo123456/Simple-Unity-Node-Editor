using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BaseNodeGraphEditor<T> : BaseNodeEditor<T> where T : BaseNodeGraphData
{

	public delegate void nodeConnection ( ConnectNodesStatus connectedStatus, int fromNodeId, int fromSlotId, int toNodeId, int toSlotId);
	public event nodeConnection NodeConnection;

	public enum ConnectNodesType { To, From, Cancel }
	public enum ConnectNodesStatus { Started, Canceled, Connected, Disconnected, Failed }

	protected virtual int EdgeExtendMargin { get => 25; }
	protected virtual float EdgeExtendSpeed { get => 0.333f; }
	protected virtual float EdgeScrolSpeed { get => 2.5f; }
	protected int selectedNodeId = -1;

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

	protected override void CalculatePanelInnerRect ()
	{


		Vector2 minPosition = Vector2.zero;
		Vector2 maxPosition = Vector2.zero;

		
		for ( int i = 0; i < nodes.Count; i++ )
		{
			// find the min and max node position, to update the scroll view inner size
			Vector2 nodeSize = nodes[ i ].GetNodeSize();
			Vector2 nodePosition = nodes[i].GetNodePosition();

			Vector2 minNodePosition = nodePosition - nodeSize;
			Vector2 maxNodePosition = nodePosition + ( nodeSize * 2f );

			minPosition.x = Mathf.Min( minPosition.x, minNodePosition.x );
			minPosition.y = Mathf.Min( minPosition.y, minNodePosition.y );

			maxPosition.x = Mathf.Max( maxPosition.x, maxNodePosition.x );
			maxPosition.y = Mathf.Max( maxPosition.y, maxNodePosition.y );

		}

		// update scroll view inner size :)
		Vector2 innerPanelSize = maxPosition - minPosition;

		// make sure that the inner pannel size is not smaller that the panel its self.
		innerPanelSize.x = Mathf.Max( innerPanelSize.x, panelRect.width );
		innerPanelSize.y = Mathf.Max( innerPanelSize.y, panelRect.height );

		panelInnerRect.size = innerPanelSize;

		panelScrollPosition.x = (( Mathf.Abs( minPosition.x ) / innerPanelSize.x) * innerPanelSize.x);
		panelScrollPosition.y = (( Mathf.Abs( minPosition.y ) / innerPanelSize.y) * innerPanelSize.y);
		lastScrolBarPosition = panelScrollPosition;	// prevent the nodes from having there position updating
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
		if ( nodePosition.x + nodeSize.x > panelInnerRect.width - EdgeExtendMargin )
		{
			extendAmount.x = nodePosition.x + nodeSize.x - (panelInnerRect.width - EdgeExtendMargin);

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

		if ( nodePosition.y + nodeSize.y > panelInnerRect.height - EdgeExtendMargin )
		{
			extendAmount.y = nodePosition.y + nodeSize.y - ( panelInnerRect.height - EdgeExtendMargin );

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

		panelInnerRect.size += extendAmount * EdgeExtendSpeed;
		panelScrollPosition += scrolAmount;
		MoveAllNodes( moveNodeAmount - scrolAmount, nodeId );

	}

	public override void RemoveNode ( int id )	// TODO: remove!
	{
		Debug.Log( "b4 Node Count: " + nodes.Count );
		base.RemoveNode( id ); 
		Debug.Log( "af Node Count: " + nodes.Count );
		
	}

	protected override Rect ClampNodePosition ( Rect nodeRect, int winId = 0 )
	{

		if ( nodeRect.x < panelRect.x - panelScrollPosition.x ) nodeRect.x = panelRect.x - panelScrollPosition.x;
		if ( nodeRect.y < panelRect.y - panelScrollPosition.y ) nodeRect.y = panelRect.y - panelScrollPosition.y;

		return nodeRect;
	}

	protected virtual void MoveAllNodes(Vector2 moveDelta, int except = -1 )	// TODO: Move to base?
	{
		for ( int i = 0; i < nodes.Count; i++ )
		{
			if ( i == except ) continue;

			nodes[ i ].MoveNode(moveDelta);

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
			else																										// Compleat Connection.
			{
				bool connectedStatus = nodes[ connectingFromNode ].AddConnection( connectingFromSlot, connectingToNode, connectingToSlot );
				if ( connectedStatus )
					NodeConnection?.Invoke( ConnectNodesStatus.Connected, connectingFromNode, connectingFromSlot, connectingToNode, connectingToSlot );
				else
					NodeConnection?.Invoke( ConnectNodesStatus.Failed, connectingFromNode, connectingFromSlot, connectingToNode, connectingToSlot );
			}

			// reset connecting :)
			ClearConnectNodes();

		}
		else if ( connectingFromNode != -1 )	// start connection
		{
			// Draw curve to mouse.
			Color curveColour = nodes[ connectingFromNode ].NodeConnections_output[ connectingFromSlot ].pinColor;
			// Fake the output to mouse when connecting nodes.
			NodePin_Output curve = new NodePin_Output(0, null, "", nodes[ connectingFromNode ].BezierControlePointOffset, curveColour, -1 );

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

	public Vector2 input_pinSize = new Vector2 ( 20, 18 );
	public Vector2 output_pinSize = new Vector2( 20, 18 );

	public virtual NodePin_Output.BezierControlePointOffset BezierControlePointOffset { get => NodePin_Output.BezierControlePointOffset.Horizontal; }

	public BaseNodeGraphData ( string _title, bool _dragable ) : base(_title, _dragable) 
	{
		GenerateNodeSize();
	}

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
		input_pinSize = _pinSize;
		output_pinSize = _pinSize;
	}

	#region BaseNodeData

	protected override void GenerateNodeSize ()
	{
		Vector2 nodeSize = new Vector2( 300, 30 );

		// get the node size by the amount of pins it has.
		float maxPinPos = Mathf.Max( GetPinLocalPosition( GetPinCount( BaseNodeGraphData.PinMode.Input ) - 1, BaseNodeGraphData.PinMode.Input ).y,
									 GetPinLocalPosition( GetPinCount( BaseNodeGraphData.PinMode.Output ) - 1, BaseNodeGraphData.PinMode.Output ).y );

		nodeSize.y += maxPinPos;

		rect.size = nodeSize;

	}

	public virtual void GeneratePinSizeAndPosition() // TODO: remove the param nodeSize, and use function GetNodeSize (from base class :/)
	{

		GenerateNodeSize();


		output_pinSize.x = rect.width / 4f;

		inputPin_localStartPosition.x = -output_pinSize.x;
		inputPin_localStartPosition.y = 5;

		outputPin_localStartPosition.x = -output_pinSize.x + rect.width - output_pinSize.x;
		outputPin_localStartPosition.y = 5;

	}

	protected override void NodeListChangeAction ( int fromId, int toId )
	{
		// update all the node connections for each output pin!

		for (int oPinId = 0; oPinId < nodeConnections_output.Count; oPinId++ )
		{

			if (toId < 0)   // node removed
			{
				nodeConnections_output[ oPinId ].UpdateConnectedNodeIds( fromId, -1, true );	// decrese all connections from FromId, removing FromId connections
			}
			else if ( fromId < 0 ) // node added
			{
				nodeConnections_output[ oPinId ].UpdateConnectedNodeIds( toId, 1, false );		// increse all connections from toId
			}
			else // node moved.
			{
				// this could be optermized a lil better :) ie. only update the range
				nodeConnections_output[oPinId].UpdateConnectedNodeIds( fromId, -1, false );     // decrese all connections from FromId, keeping FromId connections
				nodeConnections_output[oPinId].UpdateConnectedNodeIds( toId, 1, false );        // increse all connections from toId
			}

		}
		
	}

	protected override Rect GetNodeContentsPosition ()
	{
		Vector2 nodeSize = rect.size;

		return new Rect()
		{
			x = nodeSize.x / 4f,
			y = 18,
			width = nodeSize.x / 2f,
			height = nodeSize.y - 36
		};
	}

	public override void DrawNode ()
	{
		base.DrawNode();

		DrawNodePins();

	}

	#endregion 

	protected virtual void DrawNodePins ()
	{

		// inputs pins are drawn on the lhs of the node.
		// output pins are drawn on the rhs of the node.

		NodePin_Input[] nodeInputPins = NodeConnections_input.ToArray();
		NodePin_Input[] nodeOutputPins = NodeConnections_output.ToArray(); // connections output inherit from input and we only need the data that inputs class have.

		int maxPins = Mathf.Max( nodeOutputPins.Length, nodeInputPins.Length );

		for ( int i = 0; i < maxPins; i++ )
		{
			string lable = "";
			Rect pinRect;

			if ( i < nodeInputPins.Length )
			{
				lable = nodeInputPins[ i ].connectionLable;
				pinRect = GetPinRect( i, BaseNodeGraphData.PinMode.Input );

				GUI.Box( pinRect, "", guiSkin.GetStyle( "nodePin_box" ) );

				GUI.Label( pinRect, "#" );
				pinRect.x += 12;
				GUI.Label( pinRect, lable );
			}

			if ( i < nodeOutputPins.Length )
			{
				lable = nodeOutputPins[ i ].connectionLable;
				pinRect = GetPinRect( i, BaseNodeGraphData.PinMode.Output );

				GUI.Box( pinRect, "", guiSkin.GetStyle( "nodePin_box" ) );

				// add the width no the node pin (hash)  is on the right side of pinRect
				pinRect.x -= 3;
				GUI.Label( pinRect, "#", guiSkin.GetStyle( "nodeOutPin_text" ) );

				pinRect.x -= 12;
				GUI.Label( pinRect, lable, guiSkin.GetStyle( "nodeOutPin_text" ) );
			}

		}

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

	/// <summary>
	/// Addes a new pin to nodes, maintatining the pin count limits
	/// </summary>
	/// <param name="connectionLable"> the lable to be displaed on the pin </param>
	/// <param name="pinMode"> is it an input or output pin </param>
	/// <param name="connectionLimits"> The max connections allow on the pin < 0 is unlimited </param>
	public void AddPin (string connectionLable, PinMode pinMode, int connectionLimits = -1) 
	{

		if ( !CanAddPin( pinMode ) ) return;

		if ( pinMode == PinMode.Input )
			nodeConnections_input.Add( new NodePin_Input( nodeConnections_input.Count, this, connectionLable, connectionLimits ) );
		else
			nodeConnections_output.Add( new NodePin_Output( nodeConnections_output.Count, this, connectionLable, BezierControlePointOffset, connectionLimits ) );

		GeneratePinSizeAndPosition();

	}

	/// <summary>
	/// Addes an output pin to the node.
	/// </summary>
	/// <param name="connectionLable"> the lable to be displayed on the pin </param>
	/// <param name="pinColor"> the color of pin </param>
	/// <param name="connectionLimits"> The max connections allow on the pin < 0 is unlimited </param>
	[System.Obsolete] // i think that types will make this obsolete :) (as the color will be defined by the pin type.)
	public void AddOutputPin ( string connectionLable, Color pinColor, int connectionLimits = -1 )
	{

		if ( CanAddPin( PinMode.Output ) )
			nodeConnections_output.Add( new NodePin_Output( nodeConnections_output.Count, this, connectionLable, BezierControlePointOffset, pinColor, connectionLimits ) );

		GeneratePinSizeAndPosition();

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

		GeneratePinSizeAndPosition();
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
		Vector2 pinOffset;

		if ( pinMode == PinMode.Output )
			pinOffset = output_pinSize;
		else
			pinOffset = input_pinSize;

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
		Vector2 pinSize = pinMode == PinMode.Output ? output_pinSize : input_pinSize;
		return new Rect( GetPinLocalPosition( pinId, pinMode ), pinSize );
	}

	/// <summary>
	/// Adds a connection from this nodes output toNodeIds slot input. <br />
	/// Warning: connection checks are disabled during graph initalization
	/// </summary>
	/// <param name="fromSlotId"> the output slot on this pin</param>
	/// <param name="toNodeId"> the node we want to connect to </param>
	/// <param name="toSlotId"> the input slot id on the node we want to connect to </param>
	/// <returns> returns true if seccessful false otherwise </returns>
	public virtual bool AddConnection (int fromSlotId, int toNodeId, int toSlotId)
	{
		// check that the toNodeId/SlotId can except the connection

		BaseNodeGraphData toNode = (BaseNodeGraphData)GetOtherNodeFromGrph( toNodeId );

		if ( GraphIsInitalized() )
		{
			if ( toNode == null )
			{
				Debug.LogErrorFormat( "Unable to connect to node {0}. Does not exist.", toNodeId );
				return false;
			}
			else if ( toNode.GetPinCount( BaseNodeGraphData.PinMode.Input ) <= toSlotId )
			{
				Debug.LogErrorFormat( "Unable to connect to node {0}. Slot {1} does not exist.", toNodeId, toSlotId );
				return false;
			}
			else if ( !toNode.NodeConnections_input[ toSlotId ].CanConnect() )
			{
				Debug.LogErrorFormat( "Unable to connect to node {0}. Slot {1} connection limits reached.", toNodeId, toSlotId );
				return false;
			}
			else if ( GetPinCount( BaseNodeGraphData.PinMode.Output ) <= fromSlotId )
			{
				Debug.LogErrorFormat( "Unable to connect to node {0}. Slot {1} does not exist.", Id, fromSlotId );
				return false;
			}
		}

		return nodeConnections_output[ fromSlotId ].AddConnection( toNodeId, toSlotId );
	}

	public bool HasConnection( int from_pinId, int toNodeId, int toSlotId )
	{

		return nodeConnections_output[ from_pinId ].HasConnection( toNodeId, toSlotId );		

	}

	public void RemoveConnection( int pinId, int toNodeId, int toNodeSlot )
	{
		nodeConnections_output[ pinId ].RemoveConnection( toNodeId, toNodeSlot );
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
				return new Vector2( 0, input_pinSize.y / 2f );
			case PinMode.Output:
				return new Vector2( output_pinSize.x, output_pinSize.y / 2f );
		}

		return Vector2.zero;

	}

}
