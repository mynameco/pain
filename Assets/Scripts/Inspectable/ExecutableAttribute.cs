using System;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class ExecutableAttribute : Attribute
{
	public bool EditMode { get; set; }
}