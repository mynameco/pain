using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ExecutableInspector : Editor
{
	protected virtual void OnEnable()
	{
		handlers = null;

		if (targets.Length != 1)
			return;

		var container = TypeContainer.GetContainer(target.GetType());
		if (Application.isPlaying)
			handlers = container.Executable;
		else
			handlers = container.ExecutableInEditMode;
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		DrawInspectorGUI();
	}

	protected virtual void DrawInspectorGUI()
	{
		if (handlers != null)
		{
			GUILayout.Box(GUIContent.none, Separator, GUILayout.ExpandWidth(true), GUILayout.Height(1f));

			foreach (var handle in handlers)
				handle(target, this);
		}
	}

	public bool IsNeedExecute(string name)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Space(60);
		var result = GUILayout.Button(name);
		GUILayout.Space(60);
		GUILayout.EndHorizontal();
		return result;
	}

	protected static GUIStyle Separator
	{
		get
		{
			if (separator == null)
			{
				separator = new GUIStyle("box");
				separator.border.top = separator.border.bottom = 1;
				separator.margin.top = separator.margin.bottom = 5;
				separator.padding.top = separator.padding.bottom = 1;
			}

			return separator;
		}
	}

	private static GUIStyle separator;
	private List<Action<object, object>> handlers;
}
