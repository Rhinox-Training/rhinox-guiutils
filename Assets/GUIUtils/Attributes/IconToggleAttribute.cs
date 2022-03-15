using System;
using System.Diagnostics;

namespace Rhinox.GUIUtils.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    [Conditional("UNITY_EDITOR")]
    public class IconToggleAttribute : Attribute
    {
        public string TrueIcon;
        public string FalseIcon;
        
        public IconToggleAttribute(string icon)
        {
            TrueIcon = icon;
            FalseIcon = icon;
        }
    
        public IconToggleAttribute(string trueIcon, string falseIcon)
        {
            TrueIcon = trueIcon;
            FalseIcon = falseIcon;
        }
    }
}
