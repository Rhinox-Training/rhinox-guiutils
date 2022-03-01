using System;
using System.Diagnostics;

namespace Rhinox.GUIUtils.Odin
{
    [AttributeUsage(AttributeTargets.All)]
    [Conditional("UNITY_EDITOR")]
    public class TagSelectorAttribute : Attribute
    {
    }
}
