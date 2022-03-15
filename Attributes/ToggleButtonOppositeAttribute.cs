using System;
using System.Diagnostics;

namespace Rhinox.GUIUtils.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    [Conditional("UNITY_EDITOR")]
    public class ToggleButtonOppositeAttribute : Attribute
    {
        public string OppositeName;

        public ToggleButtonOppositeAttribute(string oppositeName)
        {
            OppositeName = oppositeName;
        }
    }
}