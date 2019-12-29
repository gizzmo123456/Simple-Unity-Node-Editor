using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseVerticalEditor : BaseNodeEditor<BaseVerticalNodeData>
{
    public virtual float nodeHeight { get => 40; }

    public BaseVerticalEditor () : base() {
        
        nodeReleased += NodeReleased;
    }
    public BaseVerticalEditor(Rect pannelRect) : base(pannelRect) {
        
        nodeReleased += NodeReleased;
    }

    protected override Vector2 NodeSize ()
    {
        return new Vector2( panelRect.width, nodeHeight );
    }

    protected override Vector2 NodeStartPosition ()
    {
        return new Vector2( 0, nodes.Count  * nodeHeight );
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

    protected override Rect ClampNodePosition(Rect rect, int winId)
    {
        rect.x = 0;

        if ( rect.y < 0 )
        {
            rect.y = 0;
            NodeReleased( winId );  // Temp fix for issue in BaseNodeEditor.NodeWindow
        }
        else if ( rect.y > nodeHeight * ( nodes.Count - 1 ) )
        {
            rect.y = nodeHeight * ( nodes.Count - 1 );
            NodeReleased( winId );  // Temp fix for issue in BaseNodeEditor.NodeWindow
        }

        return rect;
    }

    protected virtual void NodeReleased( int winId ) 
    {
        Vector2 winPos = nodes[ winId ].GetNodePosition();
        int lastId = nodes[ winId ].yId;
        int newId = Mathf.FloorToInt( winPos.y / nodeHeight );

        winPos.y = newId * nodeHeight;

        if ( lastId != newId )
        {

            int startId = Mathf.Min( lastId, newId );
            int endId = Mathf.Max( lastId, newId );

            // move all nodes in between last and new id                 
            for ( int i = 0; i < nodes.Count; i++)
            {
                if ( i == winId ) continue;

                int yId = nodes[i].yId;

                if ( yId >= startId && yId <= endId )
                {
                    Vector2 nPos = nodes[ i ].GetNodePosition();
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
                    nodes[ i ].SetNodePosition( nPos );
                    nodes[ i ].yId = yId;
                }
                    
            }
        }

        nodes[ winId ].SetNodePosition( winPos );
        nodes[ winId ].yId = newId;

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