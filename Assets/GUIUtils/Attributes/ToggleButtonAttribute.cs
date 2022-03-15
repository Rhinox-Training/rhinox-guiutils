using System;
using System.Diagnostics;
using Sirenix.OdinInspector;

namespace Rhinox.GUIUtils.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    [Conditional("UNITY_EDITOR")]
    public class ToggleButtonAttribute : PropertyGroupAttribute
    {
        public ToggleButtonAttribute() : base("DefaultToggleButtons") {}
        
        public ToggleButtonAttribute(string groupId) : base(groupId)
        {
        }
    }
}
