using System.Collections;
using System.Collections.Generic;
using UnityEngine;


interface INodeConnectionData
{
	void SetConnectedNodeId ( int nodeId );
	void SetConnectedSlotId ( int slotId );
	int GetConnectedNodeId ();
	int GetConnectedSlotId ();

	void UpdateNodeId ( int amountToUpdate );
	void UpdateSlotId ( int amountToUpdate );

	NodeConnectionType GetConnectionType ();

	bool CompareConnection ( NodeConnectionData otherConnection );
	bool CompareConnectionType ( NodeConnectionData otherConnection );


}

public enum NodeConnectionType{ Input, Output }

public struct NodeConnectionData : INodeConnectionData
{
	public int connectedNodeId;     // the node id to connect to.
	public int connectedSlotId;     // the input slot that the node is connected to.

	public NodeConnectionType connectionType;

	public Vector2[] connectionCurve;
	public Vector2 inputPin_startPosition;

	public NodeConnectionData ( NodeConnectionType connType, int connNodeId, int connSlotId, int curvePoints = -1 )
	{
		connectedNodeId = connNodeId;
		connectedSlotId = connSlotId;
		connectionType = connType;
		inputPin_startPosition = Vector2.zero;

		if ( curvePoints >= 0 )
			connectionCurve = new Vector2[ curvePoints + 1 ];
		else
			connectionCurve = new Vector2[ 0 ];

	}

	#region INodeConnectionData implermentation

	#region Sets

	public void SetConnectedNodeId ( int newNodeId )
	{
		connectedNodeId = newNodeId;
	}

	public void SetConnectedSlotId ( int newSlotId )
	{
		connectedSlotId = newSlotId;
	}

	#endregion

	#region Gets

	public int GetConnectedNodeId ()
	{
		return connectedNodeId;
	}

	public int GetConnectedSlotId ()
	{
		return connectedSlotId;
	}

	public NodeConnectionType GetConnectionType()
	{
		return connectionType;
	}

	#endregion

	#region Updates

	public void UpdateNodeId ( int amountToUpdate )
	{
		connectedNodeId += amountToUpdate;
	}

	public void UpdateSlotId ( int amountToUpdate )
	{
		connectedSlotId += amountToUpdate;
	}

	#endregion

	#endregion

	public void SetStartPosition ( Vector2 position )
	{
		inputPin_startPosition = position;
	}

	public bool PinMoved ( Vector2 position )
	{

		bool moved = position != inputPin_startPosition;
		return moved;

	}

	public bool CompareConnection ( NodeConnectionData otherConnection )
	{
		return connectionType  == otherConnection.connectionType  &&
			   connectedNodeId == otherConnection.connectedNodeId &&
			   connectedSlotId == otherConnection.connectedSlotId;
	}

	public bool CompareConnectionType ( NodeConnectionData otherConnection )
	{
		return connectionType == otherConnection.connectionType;
	}

}