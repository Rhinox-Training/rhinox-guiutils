using System;
using System.Diagnostics;

namespace Rhinox.GUIUtils.Attributes
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    [Conditional("UNITY_EDITOR")]
    public sealed class OptionalValueAttribute : Attribute
    {
        public string Tooltip;
    
        public OptionalValueAttribute(string tooltip = null)
        {
            Tooltip = tooltip;
        }
    }
}
