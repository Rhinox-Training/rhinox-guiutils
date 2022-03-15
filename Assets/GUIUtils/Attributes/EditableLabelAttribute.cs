using System;
using System.Diagnostics;

namespace Rhinox.GUIUtils.Attributes
{
    [AttributeUsage(AttributeTargets.All)]
    [Conditional("UNITY_EDITOR")]
    public class EditableLabelAttribute : Attribute
    {
        public string LabelProperty;
        
        public EditableLabelAttribute(string labelProp)
        {
            LabelProperty = labelProp;
        }
    }
}