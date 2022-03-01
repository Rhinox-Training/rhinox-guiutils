using System;
using System.Diagnostics;

namespace Rhinox.GUIUtils.Odin
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter)]
    [Conditional("UNITY_EDITOR")]
    public class SearchPropertyAttribute : Attribute
    {
    }

}
