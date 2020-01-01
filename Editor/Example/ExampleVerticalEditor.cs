using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleVerticalEditor : BaseVerticalEditor<ExampleVerticalNodeData>
{

    public ExampleVerticalEditor ( int uid ) : base( uid ) { }
    public ExampleVerticalEditor ( int uid, Rect pannelRect ) : base( uid, pannelRect ) { }

    public override ExampleVerticalNodeData AddNode ( string title, bool isDragable )
    {
        ExampleVerticalNodeData data = new ExampleVerticalNodeData()
        {
            dragable = isDragable,
            title = title,
            yId = nodes.Count
        };

        return AddNode( data );

    }

    protected override void NodeWindow ( int windowId )
    {
        base.NodeWindow( windowId );

    }

    public override void DrawNodeUI (int nodeId)
    {
        GUI.Label( new Rect(0, 0, 500, 20), nodes[nodeId].exampleString );
    }

}

public class ExampleVerticalNodeData : BaseVerticalNodeData
{
    public string exampleString = "Helloo World";
}