﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseVerticalEditor<T> : BaseNodeEditor<T> where T : BaseVerticalNodeData
{

    public delegate void nodeMoved ( int fromId, int toId );
    public event nodeMoved NodeMoved;

    public virtual float nodeHeight { get => 40; }
    public override Vector2 topLeftpadding { get => new Vector2( 25, 18 ); }

    public BaseVerticalEditor (int uid) : base(uid) {
        
        nodePressed += NodeReleased;
    }
    public BaseVerticalEditor(int uid, Rect pannelRect) : base(uid, pannelRect) {
        
        nodePressed += NodeReleased;
    }

    protected override void DrawNode ( int nodeId )
    {
        base.DrawNode( nodeId );

        // Add Id to the left of the node
        Rect lableRect = nodes[ nodeId ].NodeRect;
        lableRect.x -= 20;
        lableRect.width = 20;
        GUI.Label( lableRect, nodes[ nodeId ].yId.ToString() );

    }

    protected override void CalculatePanelInnerRect ()
    {
        // make the inner pannel of the pannel view the same height as the amount of nodes
        panelInnerRect = new Rect( Vector2.zero, new Vector2(panelRect.width, nodes.Count * nodeHeight) );
    }

    protected override Vector2 NodeStartPosition ()
    {
        return new Vector2( panelRect.x, panelRect.y + nodes.Count  * nodeHeight );
    }

    public override T AddNode ( T data )
    {

        data.yId = nodes.Count;

        return base.AddNode( data );
    }

    protected override Rect ClampNodePosition(Rect rect, int nodeId)
    {

        rect.x = panelRect.x;
        
        if ( rect.y < panelRect.y - panelScrollPosition.y )
        {
            rect.y = panelRect.y - panelScrollPosition.y;
        }
        else if ( rect.y > (panelRect.y - panelScrollPosition.y) + (nodeHeight * ( nodes.Count - 1 )) )
        {
            rect.y = ( panelRect.y - panelScrollPosition.y ) + ( nodeHeight * ( nodes.Count - 1 ) );
            
        }
        
        return rect;
    }

    protected virtual void NodeReleased( int nodeId, Vector2 mousePosition, bool pressed ) 
    {

        if ( pressed ) return;

        Vector2 winPos = ( nodes[ nodeId ].GetNodePosition() - panelRect.position + panelScrollPosition );

        int lastId = nodes[ nodeId ].yId;
        int newId = Mathf.FloorToInt( winPos.y / nodeHeight );

        winPos.y = newId * nodeHeight;

        if ( lastId != newId )
        {

            int startId = Mathf.Min( lastId, newId );
            int endId = Mathf.Max( lastId, newId );

            // move all nodes in between last and new id                 
            for ( int i = 0; i < nodes.Count; i++)
            {
                if ( i == nodeId ) continue;

                int yId = nodes[i].yId;

                if ( yId >= startId && yId <= endId )
                {
                    Vector2 nPos = nodes[ i ].GetNodePosition() ;
                    if (startId == lastId)
                    {
                        nPos.y -= nodeHeight;
                        yId--;
                    }
                    else 
                    {
                        nPos.y += nodeHeight;
                        yId++;

                    }
                    nodes[ i ].SetNodePosition( nPos ) ;
                    nodes[ i ].yId = yId;
                }
                    
            }

            NodeMoved?.Invoke(lastId, newId);

        }

        nodes[ nodeId ].SetNodePosition( winPos + panelRect.position - panelScrollPosition );
        nodes[ nodeId ].yId = newId;

    }

}

public abstract class BaseVerticalNodeData : BaseNodeData
{

    public int yId;

    protected Vector2 nodeSize;     // NOTE: it might be worth having a method back to BaseNodeEditor to get the Viewport info. ie position, scrol position & inner rect size.

    public override string NodeStyleName => "vert_node";


    public BaseVerticalNodeData ( bool _dragable, Vector2 _nodeSize ) : base( "", _dragable)
    {
        nodeSize = _nodeSize;
    }

	#region BaseNodeData

	protected override void GenerateNodeSize ()
    {
        rect.size = nodeSize;
    }

    // TODO: Add default action when nodes are reordered. ir override NodeListChangeAction

    protected override Rect GetNodeContentsPosition ()
    {
        float uiWidth = rect.size.x - 60;
        return new Rect( 40, 12, rect.width, rect.height - 12 );
    }

    public override Rect GetNodeDragableArea ( )
    {
        return new Rect( Vector2.zero, new Vector2( 50, nodeSize.y ) );
    }

    #endregion;
}