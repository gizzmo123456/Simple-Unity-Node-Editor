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

    public override void Update ()
    {
    }

    protected override Vector2 NodeSize ()
    {
        return new Vector2( pannelRect.width, nodeHeight );
    }

    protected override Vector2 NodeStartPosition ()
    {
        return new Vector2( 0, nodes.Count  * nodeHeight );
    }

    public override void AddNode ( string title, bool isDragable )
    {
        BaseVerticalNodeData data = new BaseVerticalNodeData()
        {
            dragable = isDragable,
            title = title,
            yId = nodes.Count
        };

        AddNode( data );

    }

    public override void AddNode ( BaseVerticalNodeData data )
    {
        data.rect = new Rect( NodeStartPosition(), NodeSize() );

        nodes.Add( data );

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
        Rect rect = nodes[ winId ].rect;
        int lastId = nodes[ winId ].yId;
        int newId = Mathf.FloorToInt( rect.y / nodeHeight );

        rect.y = newId * nodeHeight;

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
                    Rect nRect = nodes[ i ].rect;
                    if (startId == lastId)
                    {
                        nRect.y -= nodeHeight;
                        yId--;
                    }
                    else 
                    {
                        nRect.y += nodeHeight;
                        yId++;

                    }
                    nodes[ i ].rect = nRect;
                    nodes[ i ].yId = yId;
                }
                    
            }
        }

        nodes[ winId ].rect = rect;
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