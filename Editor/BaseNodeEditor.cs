﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public abstract class BaseNodeEditor<T> where T : BaseNodeData
{
    public delegate void nodeSelected ( int winId );
    public event nodeSelected nodePressed;
    public event nodeSelected nodeReleased;

    public Rect panelRect { get; set; }
    private Vector2 panelScrollPosition;

    protected List<T> nodes;
    protected int pressedNode = -1; // < 0 == none

    public BaseNodeEditor ()
    {
        nodes = new List<T>();
    }

    public BaseNodeEditor (Rect holdRect) : this ()
    {
        panelRect = holdRect;
    }

    public virtual void Update () { }

    /// <summary>
    /// Draws the node window to editorWindow.
    /// </summary>
    /// <param name="window"></param>
    public virtual void Draw ( EditorWindow window ) 
    {

        panelScrollPosition = GUI.BeginScrollView( panelRect, panelScrollPosition, GetPannelViewRect() );
        window.BeginWindows();
        
        for ( int i = 0; i < nodes.Count; i++ )
        {
            nodes[ i ].NodeRect = GUI.Window( i, nodes[ i ].NodeRect, NodeWindow, nodes[i].title );
            nodes[ i ].NodeRect = ClampNodePosition( nodes[ i ].NodeRect, i );
            
        }
        
        window.EndWindows();
        GUI.EndScrollView();

    }

    /// <summary>
    /// The viewable area within the pannel. if larger than pannel rect scroll bars will be added :D
    /// </summary>
    /// <returns></returns>
    protected Rect GetPannelViewRect()
    {
        return new Rect(Vector2.zero, panelRect.size);
    }

    /// <summary>
    /// defines how a node should be sized.
    /// </summary>
    /// <returns></returns>
    protected abstract Vector2 NodeSize ();

    /// <summary>
    /// Defines where a node should be spawned
    /// </summary>
    /// <returns></returns>
    protected virtual Vector2 NodeStartPosition()
    {
        return Vector2.zero;
    }

    /// <summary>
    /// Get the Panel and scroll position offset
    /// </summary>
    /// <returns></returns>
    protected Vector2 GetNodeOffset()
    {
        Vector2 offset = panelRect.position + panelScrollPosition;

        return offset;
    }

    /// <summary>
    /// Check if local scroll view position is visable
    /// </summary>
    /// <param name="position">Local position in scroll view</param>
    protected bool LocalPositionIsVisable(Vector2 position)
    {
        position -= panelScrollPosition;
        Debug.Log( position );
        return position.x > 0f && position.x < panelRect.width && position.y > 0f && position.y < panelRect.height;

    }

    /// <summary>
    /// Check if position is visable within the scroll view
    /// </summary>
    /// <param name="position">position in Editor Window</param>
    protected bool PositionIsVisable ( Vector2 position )
    {
        position -= panelRect.position;
        
        return LocalPositionIsVisable( position );

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
    /// <summary>
    /// Adds a new node
    /// </summary>
    public abstract T AddNode ( string title, bool isDragable );

    public abstract T AddNode ( T data );

    /// <summary>
    /// removes nodes of nodeData
    /// </summary>
    public virtual void RemoveNode ( T nodeData )
    {
        nodes.Remove( nodeData );
    }

    /// <summary>
    /// removes node at id
    /// </summary>
    public virtual void RemoveNode (int id)
    {
        if ( id < 0 || id >= nodes.Count ) return;
        nodes.RemoveAt( id );
    }

    /// <summary>
    /// Clamps node rect to postion
    /// </summary>
    /// <param name="nodeRect"></param>
    /// <returns></returns>
    protected abstract Rect ClampNodePosition ( Rect nodeRect, int winId = 0 ); // NOTE: winId is only used to fix the issue in node window.

    /// <summary>
    /// draws node data for windowId
    /// </summary>
    /// <param name="windowId"></param>
    protected virtual void NodeWindow ( int windowId )
    {
        // BUG: if the cursor leaves the node when pressed the release is not triggered.
        if ( Event.current.type == EventType.MouseDown )
        {
            nodePressed?.Invoke( windowId );
            pressedNode = windowId;
            nodes[ windowId ].pressedPosition = nodes[ windowId ].GetNodePosition();
            Debug.Log( windowId + " Pressed" );
        }
        else if ( Event.current.type == EventType.MouseUp)
        {
            nodeReleased?.Invoke( windowId );
            pressedNode = -1;
            Debug.Log( windowId + " Released" );
        }
        


        if ( nodes[windowId].dragable )
        {
            GUI.DragWindow();
        }
    }

}

public class BaseNodeData
{
    private Rect rect = Rect.zero;
    public Rect NodeRect { get => rect; set => rect = value; }

    public Vector2 pressedPosition = Vector2.zero;
    public string title = "title";
    public bool dragable = true;

    public void SetNodePosition(Vector2 position)
    {
        rect.position = position;
    }

    public Vector2 GetNodePosition()
    {
        return rect.position;
    }

    public void SetNodeSize(Vector2 size)
    {
        rect.size = size;
    }

    public Vector2 GetNodeSize()
    {
        return rect.size;
    }


}