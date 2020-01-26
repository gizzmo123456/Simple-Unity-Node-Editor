using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public abstract class BaseNodeEditor<T> where T : BaseNodeData
{

	public delegate void nodeSelected ( int nodeId, Vector2 mousePosition, bool pressed );
	public event nodeSelected nodePressed;

	/// <param name="nodeId">The node id that has been added or removed</param>
	/// <param name="added">has the id been added or removed?</param>
	public delegate void nodeListChanged (int fromId, int toId);
	public event nodeListChanged NodeListChanged;

	protected GUISkin guiSkin;

	protected abstract string NodeStyleName { get; }
	/// <summary>
	/// The path to asset must exist! (the asset ie '/NodeGraphData.asset' does not need to exist)
	/// </summary>
	public virtual string SavePath { get => "Assets/Scripts/NodeGraph/Editor/SavedData/"; }
	public virtual string AssetName { get => "NodeGraphData.asset"; }

	protected int uniqueID;
	protected bool initialized = false;

	public Rect panelRect { get; set; }
	protected Rect panelInnerRect = Rect.zero;
	protected Vector2 panelScrollPosition;

	public virtual Vector2 topLeftpadding { get => new Vector2( 18, 18 ); }
	public virtual Vector2 bottomRightpadding { get => new Vector2( 18, 18 ); }


	protected List<T> nodes;
	public int NodeCount => nodes.Count;
	protected int pressedNode = -1; // < 0 == none
	protected int releasedNode = -1; // < 0 == none

	protected Vector2 lastScrolBarPosition = Vector2.zero;

	public bool repaint = false;

	public BaseNodeEditor (int uid)
	{
		uniqueID = uid * 1000;
		nodes = new List<T>();
		guiSkin = Resources.Load<GUISkin>( "NodeEditor" );

	}

	public BaseNodeEditor (int uid, Rect holdRect) : this (uid)
	{
		panelRect = holdRect;
	}

	public virtual void Awake ( EditorWindow window ) { }

	/// <summary>
	/// This must be called once the inital node setup is compleat.
	/// </summary>
	public virtual void Initialize()
	{
		initialized = true;
	}

	public virtual void Update ( EditorWindow window ) { }

	/// <summary>
	/// Draws the node window to editorWindow.
	/// </summary>
	/// <param name="window"></param>
	public virtual void Draw ( EditorWindow window ) 
	{

		// Draw background box
		Vector3[] v = { panelRect.position - topLeftpadding,   panelRect.position + new Vector2( panelRect.size.x + bottomRightpadding.x, -topLeftpadding.y ),
						panelRect.position + panelRect.size + bottomRightpadding, panelRect.position + new Vector2(-topLeftpadding.x, panelRect.size.y + bottomRightpadding.y) };
		Handles.DrawSolidRectangleWithOutline( v, new Color(0.8f, 0.8f, 0.8f), Color.gray );

		// get scroll position
		Rect scrollRect = panelRect;
		scrollRect.size += new Vector2( 18, 18 );

		panelScrollPosition = GUI.BeginScrollView( scrollRect, panelScrollPosition, panelInnerRect );
		GUI.EndScrollView();

		Vector2 scrolDelta = panelScrollPosition - lastScrolBarPosition;
		scrolDelta = -scrolDelta;

		// Fix nodes not calling release when cursor leaves window ( Note: we dont get the event if we do this in Update :| )
		// release the pressed node preventing it from geting drawn for one update so the cursor no longer has focus of the node
		// then trigger a repaint at the end to trigger the released node event
		if ( pressedNode > -1 && Event.current != null && !PositionIsVisable( Event.current.mousePosition  ) )
		{
			// make the release the pressed n
			releasedNode = pressedNode;
			pressedNode = -1;
		}
		else if( releasedNode > -1 )
		{
			// call the released node event
			nodePressed?.Invoke( releasedNode, Vector2.zero, false ); 
			releasedNode = -1;
		}

		// draw nodes if visable
		for ( int i = 0; i < nodes.Count; i++ )
		{
			nodes[ i ].MoveNode(scrolDelta);

			// hide node if not viable of if the node has been releassed due to the mouse leaveing the node area.
			if ( (!PositionIsVisable( nodes[ i ].GetNodeCenter() ) && pressedNode != i ) || (pressedNode < 0 && releasedNode > -1) )
				continue; 

			DrawNode( i );
			
		}

		lastScrolBarPosition = panelScrollPosition;

		// trigger repaint if node has been released.
		if ( releasedNode > -1 || repaint )
		{
			window.Repaint();
			repaint = false;
		}

	}

	protected virtual void DrawNode( int nodeId )
	{
		if ( NodeStyleName != "" )
			nodes[ nodeId ].NodeRect = GUI.Window( uniqueID + nodeId, nodes[ nodeId ].NodeRect, NodeWindow, nodes[ nodeId ].title, guiSkin.GetStyle(NodeStyleName) );
		else
			nodes[ nodeId ].NodeRect = GUI.Window( uniqueID + nodeId, nodes[ nodeId ].NodeRect, NodeWindow, nodes[ nodeId ].title);

		nodes[ nodeId ].NodeRect = ClampNodePosition( nodes[ nodeId ].NodeRect, nodeId );
	}

	/// <summary>
	/// The viewable area within the pannel. if larger than pannel rect scroll bars will be added :D
	/// </summary>
	/// <returns></returns>
	protected virtual void CalculatePanelInnerRect()
	{
		panelInnerRect = new Rect(Vector2.zero, panelRect.size*2);
	}

	protected virtual Vector2 GetPanelOffset()
	{
		return panelRect.position + panelScrollPosition;
	}

	public virtual void ScrolePanel( Vector2 scrollDelta )
	{

		panelScrollPosition += scrollDelta;

	}

	/// <summary>
	/// Get the position relevent to the inner viewport. ie taking scrol bar into acount (use node.GetNodePosition for position to editor window)
	/// </summary>
	/// <param name="nodeId"></param>
	/// <returns></returns>
	protected Vector2 GetNodePositionReleventToViewPort ( int nodeId )
	{
		// Get node position gets the position to the editor window
		return nodes[ nodeId ].GetNodePosition() + panelScrollPosition - panelRect.position;
	}

	/// <summary>
	/// Defines where a node should be spawned
	/// </summary>
	/// <returns></returns>
	protected virtual Vector2 NodeStartPosition()
	{
		return panelRect.position;
	}

	/// <summary>
	/// Set the position of the node
	/// </summary>
	public virtual void SetNodePosition(int nodeId, Vector2 position)
	{
		nodes[ nodeId ].SetNodePosition(position);
	}

	/// <summary>
	/// Check if position is visable within the scroll view
	/// </summary>
	/// <param name="position">position in Editor Window</param>
	protected bool PositionIsVisable ( Vector2 position )
	{
		//position -= panelRect.position;

		return !(position.x < panelRect.x || position.x > panelRect.x + panelRect.width ||
			   position.y < panelRect.y || position.y > panelRect.y + panelRect.height);

	}

	/// <summary>
	/// Gets node at id
	/// </summary>
	/// <returns></returns>
	public virtual T GetNode ( int id )
	{
		if ( id < 0 || id >= nodes.Count ) return null;
		return nodes[ id ];
	}

	public T GetLastNode()
	{
		return nodes[ nodes.Count - 1 ];
	}

	public virtual T AddNode ( T data )
	{

		int newNodeId = nodes.Count;

		data.Init( newNodeId, RemoveNode, GetNode, guiSkin );
		data.SetNodePosition( NodeStartPosition() );

		nodes.Add( data );

		// invoke the list changed callback befor added the new node to prevent it being updated
		if (initialized)
			NodeListChanged?.Invoke( -1, newNodeId );
		
		NodeListChanged += data.NodeListChanged;            // Give the new node Add and Remove notifications

		return data;
	}

	/// <summary>
	/// removes node at id
	/// </summary>
	/// <returns>true if node was removed </returns>
	public virtual void RemoveNode ( int nodeId )
	{
		if ( nodeId < 0 || nodeId >= nodes.Count ) return;

		NodeListChanged -= nodes[ nodeId ].NodeListChanged;     // Revoke the nodes privileges

		nodes.RemoveAt( nodeId );                               // And destroy him (or what ever the hell a node is) once and for all!! :D

		if ( initialized )
			NodeListChanged?.Invoke( nodeId, -1 );

	}

	public virtual void RemoveAllNodes()
	{
		nodes.Clear();
		initialized = false;
	}

	/// <summary>
	/// does node rect contain position
	/// </summary>
	/// <param name="nodeId"> id of node</param>
	/// <param name="position"> postion</param>
	/// <returns>true if node contains position</returns>
	public bool NodeContains ( int nodeId, Vector2 position )
	{
		return nodes[ nodeId ].NodeRect.Contains(position);
	}

	/// <summary>
	/// is position within any node
	/// </summary>
	/// <param name="position"></param>
	/// <returns> -1 if no node contatins position else node id </returns>
	public int AnyNodeContains ( Vector2 position )
	{
		for ( int i = 0; i < nodes.Count; i++ )
			if ( NodeContains( i, position ) )
				return i;

		return -1;
	}

	/// <summary>
	/// Clamps node rect to postion
	/// </summary>
	/// <param name="nodeRect"></param>
	/// <returns></returns>
	protected abstract Rect ClampNodePosition ( Rect nodeRect, int nodeId = 0 ); // NOTE: winId is only used to fix the issue in node window.

	/// <summary>
	/// draws node data for windowId
	/// </summary>
	/// <param name="winId"></param>
	protected virtual void NodeWindow ( int winId )
	{

		int nodeId = winId - uniqueID;

		// BUG: if the cursor leaves the node when pressed the release is not triggered.
		if ( Event.current.type == EventType.MouseDown )
		{
			nodePressed?.Invoke( nodeId, Event.current.mousePosition, true );
			pressedNode = nodeId;
			nodes[ nodeId ].pressedPosition = nodes[ nodeId ].GetNodePosition();
		}
		else if ( Event.current.type == EventType.MouseUp)
		{
			nodePressed?.Invoke( nodeId, Event.current.mousePosition, false );
			pressedNode = -1;
		}

		nodes[ nodeId ].DrawNode();

		if ( !nodes[ nodeId ].NodeLocked && nodes[nodeId].dragable )
		{
			GUI.DragWindow(nodes[nodeId].GetNodeDragableArea());
		}

	}

	public virtual string GetGraphSaveNameFromGameObject( GameObject go )
	{
		if ( go == null ) return "";

		string name = go.scene.name + "::" + go.name;

		if (go.transform.parent != null)
		{
			name += "::" + go.transform.parent.name;
		}

		return name;
	}
	/// <summary>
	/// Saves the current graph to 'graph name'
	/// this will overwrite any pre existing data for 'graph name'
	/// </summary>
	public virtual void SaveNodeGraph( string graphName )
	{

		if ( nodes.Count == 0 )
		{
			Debug.LogWarning( "NodeGraph: No Nodes to save :)" );
			return;
		}

		NodeGraphSaveData graphSavedData = GetNodeGraphSaveData();
		
		if ( graphSavedData == null ) return;

		bool graphUpdated = false;

		// Add/Update the current node graph
		for ( int i = 0; i < nodes.Count; i++ )
			graphUpdated |= graphSavedData.UpdateGraphData( graphName, new NodeGraphSaveData.GraphSaveGroup.Graph( nodes[ i ].GetNodePosition() ), i );


		// since i have not used a serialized object for node saved data we MUST mark the scriptable object as dirty manually
		// plus we DONT want an undo step adding for this operation.
		if ( graphUpdated )
		{
			EditorUtility.SetDirty( graphSavedData );
			AssetDatabase.SaveAssets();
		}

		Debug.Log( "NodeGraph Saved!" );

	}

	/// <summary>
	/// Loads and applies graph data for 'graph name'.
	/// Should be called after the inital nodes have been loaded
	/// </summary>
	public virtual void LoadGraphData( string graphName )
	{

		List<NodeGraphSaveData.GraphSaveGroup.Graph> graphData = GetNodeGraphSaveData()?.GetGraphData( graphName );

		if ( graphData == null )
		{
			Debug.LogWarning( "NodeGraph: No Save data found!" );
			return;
		}

		// graph data and nodes should be 1 to 1, but just in case use the min count to prevent any array index out range :)
		for ( int i = 0; i < Mathf.Min( graphData.Count, nodes.Count ); i++ )
		{
			// set position
			nodes[ i ].SetNodePosition( graphData[i].nodePosition );
		}

		CalculatePanelInnerRect();

	}

	/// <summary>
	/// Attemps to get the nodeGraphSaveData. If none is found attempts to create a new saved data.
	/// </summary>
	/// <returns></returns>
	private NodeGraphSaveData GetNodeGraphSaveData()
	{
		// attampt to load data, if does not exist creat new :)
		NodeGraphSaveData savedData = (NodeGraphSaveData)AssetDatabase.LoadAssetAtPath( SavePath+AssetName, typeof( NodeGraphSaveData ) );

		if ( savedData == null )
		{
			savedData = ScriptableObject.CreateInstance<NodeGraphSaveData>(); //new NodeGraphSaveData();
			AssetDatabase.CreateAsset( savedData, SavePath+AssetName );

			if ( savedData == null )
			{
				Debug.LogErrorFormat( "NodeGraph: Unable to create save data. Does the path '{0}' exist?", SavePath );
			}

		}

		return savedData;

	}

}

public abstract class BaseNodeData
{

	public delegate void NodeAction ( int nodeId );
	public delegate BaseNodeData GetNodeAction( int nodeId );

	protected NodeAction RemoveNodeFromGraph;
	protected GetNodeAction GetOtherNodeFromGrph;

	protected GUISkin guiSkin;

	public int Id { get; private set; }

	// Node position
	protected Rect rect = Rect.zero;
	public Rect NodeRect { get => rect; set => rect = value; }  // Set needs removing.

	public string title = "title";
	public Vector2 pressedPosition = Vector2.zero;
	
	public bool dragable = true;

	public bool LockNodeInPlayMode => true;
	public bool NodeLocked => LockNodeInPlayMode && EditorApplication.isPlaying;

	public BaseNodeData(string _title, bool _dragable)
	{
		title = _title;
		dragable = _dragable;
	}

	/// <summary>
	/// Initalizes the node, should be called directly affter adding the node!
	/// </summary>
	public virtual void Init( int nodeId, NodeAction removeNodeFunt, GetNodeAction getOtherNodeFunt, GUISkin _guiSkin)
	{

		Id = nodeId;

		RemoveNodeFromGraph = removeNodeFunt;
		GetOtherNodeFromGrph = getOtherNodeFunt;
		guiSkin = _guiSkin;

	}

	/// <summary>
	/// Method called by the graph when the node graph changes
	/// </summary>
	/// <param name="fromId"> the position the node moved from. less than 0 is added </param>
	/// <param name="toId"> the position the node was moved to, less than 0 is removed </param>
	public void NodeListChanged( int fromId, int toId )
	{

		NodeListChangeAction( fromId, toId );

		Debug.LogFormat("cID {2}, From {0}, To{1}", fromId, toId, Id);

		if ( (fromId < 0 || fromId > Id) && toId >= 0 && toId <= Id )
		{
			// node has been added or moved below this index
			++Id;
			Debug.Log("Incress");
		}
		else if (fromId >= 0 && fromId < Id && (toId < 0 || toId > Id))
		{
			// node has been removed from below or moved above this index 
			--Id;
			Debug.Log( "Decress" );
		}

	}

	/// <summary>
	/// The action to be preformed when the list of nodes changes.
	/// </summary>
	/// <param name="fromId"> the position the node moved from. less than 0 is added </param>
	/// <param name="toId"> the position the node was moved to, less than 0 is removed </param>
	protected abstract void NodeListChangeAction ( int fromId, int toId );	// iv chooses to add a function rather than overriding NodeListChanged
																			// to mimimize error. Since the Id MUST be updated!

	/// <summary>
	/// Set the node position relevent to the editor window
	/// </summary>
	/// <param name="position"> new position </param>
	public void SetNodePosition(Vector2 position)
	{
		rect.position = position;
	}

	/// <summary>
	/// Move the node by delta amount
	/// </summary>
	/// <param name="moveDelta"> amount to move node </param>
	public void MoveNode(Vector2 moveDelta)
	{
		rect.position += moveDelta;
	}

	/// <summary>
	/// Gets the position of node relevent to the editor window
	/// </summary>
	public Vector2 GetNodePosition()
	{
		return rect.position;
	}

	/// <summary>
	/// Gets the center point of the node relevent to the node window
	/// </summary>
	public Vector2 GetNodeCenter()
	{
		return rect.center;
	}

	[System.Obsolete("Function will be removed and the node size will be set internaly")]
	public void SetNodeSize(Vector2 size)
	{
		rect.size = size;
	}

	/// <summary>
	/// Generates the size the node for its current state.
	/// </summary>
	protected abstract void GenerateNodeSize ();

	/// <summary>
	/// Gets the size of the node.
	/// </summary>
	public Vector2 GetNodeSize()
	{
		return rect.size;
	}

	/// <summary>
	/// Gets the main contents area of the node; 
	/// Useful to restrict areas of the node for other things ie. pins :)
	/// </summary>
	protected abstract Rect GetNodeContentsPosition ();

	/// <summary>
	/// Main Node UI
	/// </summary>
	protected abstract void NodeUi ();

	/// <summary>
	/// Draws the nodes contents to the window.
	/// </summary>
	public virtual void DrawNode ()
	{

		GUI.BeginGroup( GetNodeContentsPosition() );

		NodeUi();

		GUI.EndGroup();

	}

	/// <summary>
	/// The position within the node that can be used to drag the node around. <br/>
	/// NOTE: Any position outside of the node will be ignored!
	/// </summary>
	/// <returns></returns>
	public virtual Rect GetNodeDragableArea ()
	{
		return new Rect( Vector2.zero, NodeRect.size );
	}

}
