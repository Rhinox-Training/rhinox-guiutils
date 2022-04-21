using System;
using System.Diagnostics;
using UnityEngine;

/// <summary>
/// Only works properly in Unity 2020.1+ (nested generics are impossible before then)
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
[Conditional("UNITY_EDITOR")]
public class DrawAsUnityGenericAttribute : PropertyAttribute
{
    
}
