using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleVerticalEditor : BaseVerticalEditor<ExampleVerticalNodeData>
{

    public ExampleVerticalEditor ( int uid ) : base( uid ) { }
    public ExampleVerticalEditor ( int uid, Rect pannelRect ) : base( uid, pannelRect ) { }

    protected override void DrawNodeUI (int nodeId)
    {
        GUI.Label( new Rect(0, 0, 500, 20), nodes[nodeId].exampleString );
    }

}

public class ExampleVerticalNodeData : BaseVerticalNodeData
{

    public ExampleVerticalNodeData ( bool _dragable, string text, Vector2 _nodeSize ) : base( _dragable, _nodeSize ) { exampleString = text; }

    public string exampleString = "Helloo World";
}