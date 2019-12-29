using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public abstract class BaseNodeEditor<T> where T : BaseNodeData
{
    public delegate void nodeSelected ( int winId );
    public event nodeSelected nodePressed;
    public event nodeSelected nodeReleased;

    public Rect pannelRect { get; set; }

    protected List<T> nodes;
    protected int pressedNode = -1; // < 0 == none

    public BaseNodeEditor ()
    {
        nodes = new List<T>();
    }

    public BaseNodeEditor (Rect holdRect) : this ()
    {
        pannelRect = holdRect;
    }

    public abstract void Update ();

    /// <summary>
    /// Draws the node window to editorWindow.
    /// </summary>
    /// <param name="window"></param>
    public void Draw ( EditorWindow window ) 
    {

        window.BeginWindows();
        
        for ( int i = 0; i < nodes.Count; i++ )
        {
            nodes[ i ].rect = GUI.Window( i, nodes[ i ].rect, NodeWindow, nodes[i].title );
            nodes[ i ].rect = ClampNodePosition( nodes[ i ].rect, i );
            
        }
        
        window.EndWindows();

    }


    protected abstract Vector2 NodeSize ();

    protected virtual Vector2 NodeStartPosition()
    {
        return Vector2.zero;
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
    public abstract void AddNode ( string title, bool isDragable );

    public abstract void AddNode ( T data );

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
            nodes[ windowId ].pressedPosition = nodes[ windowId ].rect.position;
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
    public Rect rect;
    public Vector2 pressedPosition;
    public string title;
    public bool dragable = true;
}