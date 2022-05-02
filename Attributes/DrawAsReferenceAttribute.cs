using System;
using System.Diagnostics;
using UnityEngine;

namespace Rhinox.GUIUtils.Attributes
{
    // TODO class usage
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class)]
    [Conditional("UNITY_EDITOR")]
    public class DrawAsReferenceAttribute : PropertyAttribute
    {
        
    }
}