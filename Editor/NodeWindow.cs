using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NodeWindow : EditorWindow
{

	[MenuItem( "Nodes/NodeEditor" )]
	public static void ShowWindow ()
	{
		GetWindow( typeof( NodeWindow ) );
	}

	BaseVerticalEditor nodeWindow;

	private void Awake ()
	{

		nodeWindow = new BaseVerticalEditor(new Rect(0, 0, 500, 500));
		nodeWindow.AddNode("Win 1", true);
		nodeWindow.AddNode("Win 2", true);
		nodeWindow.AddNode("Win 3", true);
		nodeWindow.AddNode("Win 4", true);
		nodeWindow.AddNode("Win 5", true);

	}

	private void Update ()
	{
		nodeWindow.Update();
	}

	private void OnGUI ()
	{
		
		nodeWindow.Draw(this);
	}



}
