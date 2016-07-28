using System;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public class InspectableAttribute : Attribute
{
	public bool Mutable { get; set; }
	public bool Name { get; set; }
}