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

}

struct Input_NodeConnectionData : INodeConnectionData
{
	public int connectedNodeId;
	public int connectedSlotId;

	public Input_NodeConnectionData ( int nodeId, int slotId )
	{
		connectedNodeId = nodeId;
		connectedSlotId = slotId;
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

}

struct Output_NodeConnectionData : INodeConnectionData
{
	public int connectedNodeId;     // the node id to connect to.
	public int connectedSlotId;     // the input slot that the node is connected to.

	public Vector2[] connectionCurve;

	public Vector2 inputPin_startPosition;

	public Output_NodeConnectionData ( int connNodeId, int connSlotId, int curvePoints )
	{
		connectedNodeId = connNodeId;
		connectedSlotId = connSlotId;
		connectionCurve = new Vector2[ curvePoints + 1 ];
		inputPin_startPosition = Vector2.zero;

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

}