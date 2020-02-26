using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorUiHelpers
{

	public static bool RectButton ( Rect rect, Vector2 mousePosition, bool pressed )
	{

		if ( pressed && rect.Contains( mousePosition ) )
			return true;

		return false;

	}

}

public class EditorWindowEvent
{

	public static bool HasEvent { get; private set; }
	public static Vector2 MousePosition { get; private set; }
	public static EventType Type { get; private set; }

	/// <summary>
	/// Gets the mouse position relevent to rect position.
	/// </summary>
	public static Vector2 MousePositionRelevent( Rect rect )
	{
		return MousePosition - rect.position;
	}

	public void Update()
	{
		HasEvent = Event.current != null;
		
		if (HasEvent)
		{
			Type = Event.current.type;
			MousePosition = Event.current.mousePosition;
		}

	}

}

public class EditorRectHelper
{

	public enum RectAction { Indent, Unindent, NextRow, Space }

	float indentWidth;
	float rowHeight;
	float rowSpace;

	public EditorRectHelper(float _indentWidth, float _rowHeight, float _rowSpace)
	{
		indentWidth = _indentWidth;
		rowHeight = _rowHeight;
		rowSpace = _rowSpace;
	}

	public void UpdateRect ( ref Rect rect, params RectAction[] rectActions )
	{

		foreach ( RectAction ra in rectActions )
			switch ( ra )
			{
				case RectAction.Indent:
				rect.x += indentWidth;
				rect.width -= indentWidth;
				break;
				case RectAction.Unindent:
				rect.x -= indentWidth;
				rect.width += indentWidth;
				break;
				case RectAction.NextRow:
				rect.y += rowHeight;
				break;
				case RectAction.Space:
				rect.y += rowSpace;
				break;
			}

	}

}