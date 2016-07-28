using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;

public class InspectableInspector : ExecutableInspector
{
	protected override void OnEnable()
	{
		base.OnEnable();

		handlers = null;

		if (!Application.isPlaying)
			return;

		if (targets.Length != 1)
			return;

		var type = PrefabUtility.GetPrefabType(target);
		if (type == PrefabType.Prefab)
			return;

		var container = TypeContainer.GetContainer(target.GetType());
		handlers = container.DrawInspectable;

		if (handlers != null)
		{
			lastTime = Time.time;
			EditorApplication.update += OnUpdate;
		}
	}

	private void OnDestroy()
	{
		EditorApplication.update -= OnUpdate;
	}

	private void OnUpdate()
	{
		if ((lastTime + timeDelay) < Time.time)
		{
			lastTime = Time.time;
			if (targets.Length == 1)
			{
				Repaint();
			}
		}
	}

	public object DrawInspectable(string name, Type type, object value, ref bool changed, bool mutable)
	{
		return DrawInspectableImpl(name, type, value, ref changed, mutable);
	}

	private object DrawInspectableImpl(string name, Type type, object value, ref bool changed, bool mutable)
	{
		name = ToUpperFirstCharacter(name);

		object result = null;
		if (type == typeof(int))
		{
			GUILayout.BeginHorizontal();
			GUILayout.Space(offset * spaceOffset);
			result = EditorGUILayout.IntField(mutable ? name + "*" : name, (int)value);
			GUILayout.EndHorizontal();
		}
		else if (type == typeof(float))
		{
			GUILayout.BeginHorizontal();
			GUILayout.Space(offset * spaceOffset);
			result = EditorGUILayout.FloatField(mutable ? name + "*" : name, (float)value);
			GUILayout.EndHorizontal();
		}
		else if (type == typeof(bool))
		{
			GUILayout.BeginHorizontal();
			GUILayout.Space(offset * spaceOffset);
			result = EditorGUILayout.Toggle(mutable ? name + "*" : name, (bool)value);
			GUILayout.EndHorizontal();
		}
		else if (type == typeof(string))
		{
			GUILayout.BeginHorizontal();
			GUILayout.Space(offset * spaceOffset);
			result = EditorGUILayout.TextField(mutable ? name + "*" : name, (string)value);
			GUILayout.EndHorizontal();
		}
		else if (type == typeof(Vector2))
		{
			GUILayout.BeginHorizontal();
			GUILayout.Space(offset * spaceOffset);
			result = EditorGUILayout.Vector2Field(mutable ? name + "*" : name, (Vector2)value);
			GUILayout.EndHorizontal();
		}
		else if (type == typeof(Vector3))
		{
			GUILayout.BeginHorizontal();
			GUILayout.Space(offset * spaceOffset);
			result = EditorGUILayout.Vector3Field(mutable ? name + "*" : name, (Vector3)value);
			GUILayout.EndHorizontal();
		}
		else if (type == typeof(Vector4))
		{
			GUILayout.BeginHorizontal();
			GUILayout.Space(offset * spaceOffset);
			result = EditorGUILayout.Vector4Field(mutable ? name + "*" : name, (Vector4)value);
			GUILayout.EndHorizontal();
		}
		else if (type == typeof(Rect))
		{
			GUILayout.BeginHorizontal();
			GUILayout.Space(offset * spaceOffset);
			result = EditorGUILayout.RectField(mutable ? name + "*" : name, (Rect)value);
			GUILayout.EndHorizontal();
		}
		else if (type.IsEnum)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Space(offset * spaceOffset);
			result = EditorGUILayout.EnumPopup(mutable ? name + "*" : name, (Enum)value);
			GUILayout.EndHorizontal();
		}
		else if ((value != null && typeof(UnityEngine.Object).IsAssignableFrom(value.GetType())) ||
			(typeof(UnityEngine.Object).IsAssignableFrom(type)))
		{
			GUILayout.BeginHorizontal();
			GUILayout.Space(offset * spaceOffset);
			result = EditorGUILayout.ObjectField(mutable ? name + "*" : name, (UnityEngine.Object)value, type, true);
			GUILayout.EndHorizontal();
		}
		else if (typeof(IEnumerable).IsAssignableFrom(type) && value != null)
		{
			var list = ((IEnumerable)value).Cast<object>().ToArray();
			var nameList = mutable ? name + "*" : name;
			nameList += " : " + list.Length.ToString();

			bool expand = expanded.Contains(deepName + name);
			GUILayout.BeginHorizontal();
			GUILayout.Space(offset * spaceOffset);
			bool expand2 = EditorGUILayout.Foldout(expand, nameList);
			GUILayout.EndHorizontal();
			if (expand2)
			{
				var elementType = GetElementType(type);
				var currentOffset = offset;
				offset += 1;
				var currentName = deepName;
				deepName += name;
				DrawList(list, elementType, ref changed, mutable);
				offset = currentOffset;
				deepName = currentName;
			}
			if (expand && !expand2)
				expanded.Remove(deepName + name);
			else if (!expand && expand2)
				expanded.Add(deepName + name);
		}
		else if (type.IsClass || type.IsInterface/* || (type.IsValueType && !type.IsPrimitive)*/)
		{
			bool expand = expanded.Contains(deepName + name);
			GUILayout.BeginHorizontal();
			GUILayout.Space(offset * spaceOffset);

			elementName = "";
			if (value != null)
			{
				var container = TypeContainer.GetContainer(value.GetType());
				if (container.InspectableName != null)
				{
					foreach (var handler in container.InspectableName)
						handler(value, this);
				}
			}
			else
			{
				elementName = "null";
			}

			var name2 = mutable ? name + "*" : name;
			var name3 = string.IsNullOrEmpty(elementName) ? (name2 + " [" + (value != null ? value.GetType().Name : "") + "]") : (name2 + " [" + (value != null ? value.GetType().Name + " : " : "") + elementName + "]");

			var expand2 = EditorGUILayout.Foldout(expand, name3);
			GUILayout.EndHorizontal();

			if (expand2 && value != null)
			{
				var currentOffset = offset;
				offset += 1;
				var currentName = deepName;
				deepName += name;

				if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
				{
					var typeKey = type.GetGenericArguments()[0];
					var keyValue = type.GetProperty("Key").GetValue(value, null);
					DrawInspectableImpl("Key", typeKey, keyValue, ref changed, mutable);

					var typeValue = type.GetGenericArguments()[1];
					var valueValue = type.GetProperty("Value").GetValue(value, null);
					DrawInspectableImpl("Value", typeValue, valueValue, ref changed, mutable);
				}
				else
				{
					var container = TypeContainer.GetContainer(value.GetType());
					if (container.DrawInspectable != null)
					{
						foreach (var handler in container.DrawInspectable)
							handler(value, this);
					}
				}

				offset = currentOffset;
				deepName = currentName;
			}
			if (expand && !expand2)
				expanded.Remove(deepName + name);
			else if (!expand && expand2)
				expanded.Add(deepName + name);
		}
		else
		{
			if (value != null)
			{
				var formatable = value as IFormattable;
				GUILayout.BeginHorizontal();
				GUILayout.Space(offset * spaceOffset);
				EditorGUILayout.TextField(name, formatable != null ? formatable.ToString("", CultureInfo.InvariantCulture) : value.ToString());
				GUILayout.EndHorizontal();
			}
		}

		if (GUI.changed)
			changed = true;

		return result;
	}

	private static Type GetElementType(Type type)
	{
		if (type.HasElementType)
			return type.GetElementType();
		var args = type.GetGenericArguments();
		if (args.Length == 1)
			return args[0];
		return typeof(object);
	}

	private static string ToUpperFirstCharacter(string name)
	{
		if (name.Length != 0)
		{
			var array = name.ToCharArray();
			var first = new string(new[] { array[0] });
			first = first.ToUpper();
			if (first[0] != array[0])
			{
				array[0] = first[0];
				name = new string(array);
			}
		}
		return name;
	}

	private void DrawList(object[] items, Type type, ref bool changed, bool mutable)
	{
		int index = 0;
		foreach (var item in items)
		{
			var name = "Element " + index.ToString();
			index++;

			DrawInspectableImpl(name, type, item, ref changed, mutable);
		}
	}

	protected override void DrawInspectorGUI()
	{
		if (handlers != null)
		{
			GUILayout.Box(GUIContent.none, Separator, GUILayout.ExpandWidth(true), GUILayout.Height(1f));

			var target = (MonoBehaviour)this.target;
			offset = 0;
			deepName = "";

			foreach (var handler in handlers)
				handler(target, this);
		}

		base.DrawInspectorGUI();
	}

	public void SetName(string name)
	{
		if (string.IsNullOrEmpty(elementName))
			elementName = name;
		else
			elementName += " | " + name;
	}

	private int offset;
	private string deepName;
	private HashSet<string> expanded = new HashSet<string>();
	private float lastTime;
	private float timeDelay = 0.5f;
	private string elementName = "";
	private float spaceOffset = 16;
	private List<Action<object, object>> handlers;
}
