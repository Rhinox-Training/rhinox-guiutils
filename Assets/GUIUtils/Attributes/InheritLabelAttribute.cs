using System;
using System.Diagnostics;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Property)]
[Conditional("UNITY_EDITOR")]
public class InheritLabelAttribute : Attribute
{
}
