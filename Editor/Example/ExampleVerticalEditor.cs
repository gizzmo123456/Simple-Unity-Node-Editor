using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Obsolete]
public class ExampleVerticalEditor : BaseVerticalEditor<ExampleVerticalNodeData>
{

    public ExampleVerticalEditor ( int uid ) : base( uid ) { }
    public ExampleVerticalEditor ( int uid, Rect pannelRect ) : base( uid, pannelRect ) { }

}

public class ExampleVerticalNodeData : BaseVerticalNodeData
{

    public ExampleVerticalNodeData ( bool _dragable, string text, Vector2 _nodeSize ) : base( _dragable, _nodeSize ) { exampleText = text; }

    public string exampleText = "Helloo World";

    protected override void NodeUi ()
    {
        GUI.Label( new Rect( 0, 0, 300, 300 ), exampleText );
    }

    protected override void NodeListChangeAction ( int fromId, int toId )
    {
    }

    public override bool CompareNode ( BaseNodeData otherNode )
    {
        return ( (ExampleVerticalNodeData)otherNode ).exampleText == exampleText;
    }

}