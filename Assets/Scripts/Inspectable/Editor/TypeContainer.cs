using System;
using System.Collections.Generic;
using System.Reflection;

public class TypeContainer
{
	public List<Action<object, object>> Executable;
	public List<Action<object, object>> ExecutableInEditMode;
	public List<Action<object, object>> DrawInspectable;
	public List<Action<object, object>> InspectableName;

	public static TypeContainer GetContainer(Type type)
	{
		TypeContainer result;
		if (cache.TryGetValue(type, out result))
			return result;

		result = CreateContainer(type);
		cache.Add(type, result);
		return result;
	}

	private static TypeContainer CreateContainer(Type type)
	{
		var container = new TypeContainer();

		var currentType = type;
		while (currentType != typeof(object))
		{
			var members = currentType.GetMembers(BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			foreach (var member in members)
				ComputeMember(container, member);

			currentType = currentType.BaseType;
		}

		return container;
	}

	private static void ComputeMember(TypeContainer container, MemberInfo member)
	{
		var inspectable = (InspectableAttribute[])member.GetCustomAttributes(typeof(InspectableAttribute), false);
		if (inspectable.Length == 1)
		{
			ComputeInspectableAction(container, member, inspectable[0]);
			return;
		}

		var executable = (ExecutableAttribute[])member.GetCustomAttributes(typeof(ExecutableAttribute), false);
		if (executable.Length == 1)
			ComputeExecutableAction(container, member, executable[0]);
	}

	private static void ComputeInspectableAction(TypeContainer container, MemberInfo member, InspectableAttribute inspectable)
	{
		var field = member as FieldInfo;
		if (field != null)
		{
			AddItem(ref container.DrawInspectable,
				delegate (object _target, object _data)
				{
					var inspector = (InspectableInspector)_data;
					var changed = false;
					var value = inspector.DrawInspectable(field.Name, field.FieldType, field.GetValue(_target), ref changed, inspectable.Mutable);
					if (changed && inspectable.Mutable)
						field.SetValue(_target, value);
				});

			if (inspectable.Name)
			{
				AddItem(ref container.InspectableName,
					delegate (object _target, object _data)
					{
						var inspector = (InspectableInspector)_data;
						var value = field.GetValue(_target);

						if (value != null)
							inspector.SetName(value.ToString());
					});
			}
			return;
		}

		var property = (PropertyInfo)member;
		var hasMutator = property.GetSetMethod(true) != null;
		AddItem(ref container.DrawInspectable,
			delegate (object _target, object _data)
			{
				var inspector = (InspectableInspector)_data;
				var changed = false;
				var value = inspector.DrawInspectable(property.Name, property.PropertyType, property.GetValue(_target, null),
					ref changed, inspectable.Mutable);
				if (changed && inspectable.Mutable && hasMutator)
					property.SetValue(_target, value, null);
			});

		if (inspectable.Name)
		{
			AddItem(ref container.InspectableName,
				delegate (object _target, object _data)
				{
					var inspector = (InspectableInspector)_data;
					var value = property.GetValue(_target, null);

					if (value != null)
						inspector.SetName(value.ToString());
				});
		}
	}

	private static void ComputeExecutableAction(TypeContainer container, MemberInfo member, ExecutableAttribute executable)
	{
		var method = member as MethodInfo;
		Action<object, object> handler =
			delegate (object _target, object _data)
			{
				var inspector = (ExecutableInspector)_data;
				if (inspector.IsNeedExecute(method.Name))
					method.Invoke(_target, null);
			};
		if (executable.EditMode)
			AddItem(ref container.ExecutableInEditMode, handler);
		else
			AddItem(ref container.Executable, handler);
	}

	private static void AddItem(ref List<Action<object, object>> list, Action<object, object> item)
	{
		if (list == null)
			list = new List<Action<object, object>>();
		list.Add(item);
	}

	private static Dictionary<Type, TypeContainer> cache = new Dictionary<Type, TypeContainer>();
}
