using System;
using System.Diagnostics;
using UnityEngine;

namespace Rhinox.GUIUtils.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    [Conditional("UNITY_EDITOR")]
    public class LayerAttribute : PropertyAttribute
    { 
    }
}
