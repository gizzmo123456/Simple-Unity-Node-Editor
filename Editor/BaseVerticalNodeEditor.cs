using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseVerticalEditor : BaseNodeEditor<BaseVerticalNodeData>
{
    public virtual float nodeHeight { get => 40; }
    public override Vector2 topLeftpadding { get => new Vector2( 25, 18 ); }

    public BaseVerticalEditor (int uid) : base(uid) {
        
        nodeReleased += NodeReleased;
    }
    public BaseVerticalEditor(int uid, Rect pannelRect) : base(uid, pannelRect) {
        
        nodeReleased += NodeReleased;
    }

    protected override void DrawNode ( int nodeId )
    {
        base.DrawNode( nodeId );
        Rect lableRect = nodes[ nodeId ].NodeRect;
        lableRect.x -= 20;
        lableRect.width = 20;
        GUI.Label( lableRect, nodes[ nodeId ].yId.ToString() );
    }

    protected override Rect GetPannelViewRect ()
    {
        return new Rect( Vector2.zero, new Vector2(panelRect.width, nodes.Count * nodeHeight) );
    }

    protected override Vector2 NodeSize ()
    {
        return new Vector2( panelRect.width, nodeHeight );
    }

    protected override Vector2 NodeStartPosition ()
    {
        return new Vector2( panelRect.x, panelRect.y + nodes.Count  * nodeHeight );
    }

    public override BaseVerticalNodeData AddNode ( string title, bool isDragable )
    {
        BaseVerticalNodeData data = new BaseVerticalNodeData()
        {
            dragable = isDragable,
            title = title,
            yId = nodes.Count
        };

        return AddNode( data );

    }

    public override BaseVerticalNodeData AddNode ( BaseVerticalNodeData data )
    {
        data.SetNodePosition( NodeStartPosition() );
        data.SetNodeSize( NodeSize() );

        nodes.Add( data );

        return data;

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

    protected virtual void NodeReleased( int nodeId ) 
    {

        Vector2 winPos = ( nodes[ nodeId ].GetNodePosition() - panelRect.position + panelScrollPosition );

        int lastId = nodes[ nodeId ].yId;
        int newId = Mathf.FloorToInt( winPos.y / nodeHeight );

        winPos.y = newId * nodeHeight;
        if (lastId == 0)
            Debug.Log( ":::::::::::" + lastId + " :: " + newId );

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
        }

        nodes[ nodeId ].SetNodePosition( winPos + panelRect.position - panelScrollPosition );
        nodes[ nodeId ].yId = newId;

    }

    protected override void NodeWindow ( int windowId )
    {
        
        base.NodeWindow( windowId );
    }

}

public class BaseVerticalNodeData : BaseNodeData
{

    public int yId;

}